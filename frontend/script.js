/**
 * WEB_XuLyAnh — Frontend Script
 * Tương tác giao diện & tích hợp Backend API (ASP.NET Core Minimal API)
 * Tuân thủ nghiêm ngặt Hợp đồng API & Tài liệu bối cảnh dự án.
 */

// ============================================================================
// 1. CẤU HÌNH & TRẠNG THÁI TOÀN CỤC
// ============================================================================
const getApiBaseUrl = () => {
    // 1. Mở trực tiếp bằng file:/// -> gọi backend localhost:5000
    if (window.location.protocol === 'file:') {
        return 'http://localhost:5000';
    }
    // 2. Chạy từ VS Code Live Server (thường port 5500-5509)
    if (window.location.port.startsWith('550')) {
        return 'http://localhost:5000';
    }
    // 3. Môi trường tích hợp: dotnet run, Docker, Cloud (Render/Railway), VPS mọi port -> gọi relative ''
    return '';
};

const API_BASE = getApiBaseUrl();

// Trạng thái ứng dụng
let currentRows = 3;
let currentCols = 3;
let currentK = 3;
let currentMethod = 'mean'; // 'mean' | 'median' | 'prewitt'
let currentFormat = 'roundedInt'; // Mặc định: 'roundedInt' (Số nguyên) theo yêu cầu!
let showPadding = true; // Mặc định: BẬT hiển thị padding 0
let enableHeatmap = true; // Mặc định: BẬT phân chia màu sắc cho các số giống nhau
let lastResponseData = null;
let selectedCellCoords = { r: 0, c: 0 };

// ============================================================================
// 2. KHỞI TẠO DOM ELEMENTS
// ============================================================================
const el = {
    apiStatusBadge: document.getElementById('apiStatusBadge'),
    statusDot: document.getElementById('statusDot'),
    apiStatusText: document.getElementById('apiStatusText'),

    rows: document.getElementById('rows'),
    cols: document.getElementById('cols'),
    k: document.getElementById('k'),
    btnDecRows: document.getElementById('btnDecRows'),
    btnIncRows: document.getElementById('btnIncRows'),
    btnDecCols: document.getElementById('btnDecCols'),
    btnIncCols: document.getElementById('btnIncCols'),
    btnDecK: document.getElementById('btnDecK'),
    btnIncK: document.getElementById('btnIncK'),

    mat: document.getElementById('mat'),
    padHint: document.getElementById('padHint'),
    selectedWindowInfo: document.getElementById('selectedWindowInfo'),
    sampleBtn: document.getElementById('sample'),
    clearBtn: document.getElementById('clear'),
    pasteExcelBtn: document.getElementById('pasteExcel'),

    // Điền giá trị & Bật/Tắt padding 0
    fillVal: document.getElementById('fillVal'),
    btnFillAll: document.getElementById('btnFillAll'),
    btnTogglePad: document.getElementById('btnTogglePad'),
    padToggleIcon: document.getElementById('padToggleIcon'),
    padToggleText: document.getElementById('padToggleText'),

    // Chọn nhanh cửa sổ k
    quickKChips: document.querySelectorAll('.quick-k-chips .k-chip'),

    methodCards: document.querySelectorAll('.method-card'),
    kersSection: document.getElementById('kers'),
    gxContainer: document.getElementById('gx'),
    gyContainer: document.getElementById('gy'),
    resetKernelBtn: document.getElementById('rk'),

    runBtn: document.getElementById('run'),
    runBtnText: document.getElementById('runBtnText'),
    runSpinner: document.getElementById('runSpinner'),
    errBox: document.getElementById('err'),

    outSection: document.getElementById('out'),
    warningsBox: document.getElementById('warningsBox'),
    resContainer: document.getElementById('res'),
    detailSection: document.getElementById('detail'),
    formatToggleBtns: document.querySelectorAll('.format-toggle .toggle-btn'),

    // Tô màu Heatmap
    btnToggleHeatmap: document.getElementById('btnToggleHeatmap'),
    heatmapIcon: document.getElementById('heatmapIcon'),
    heatmapText: document.getElementById('heatmapText')
};

// ============================================================================
// 3. HEALTH CHECK BACKEND API
// ============================================================================
async function checkApiHealth() {
    try {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 2500);
        const res = await fetch(`${API_BASE}/api/health`, { signal: controller.signal });
        clearTimeout(timeoutId);

        if (res.ok) {
            el.statusDot.className = 'status-indicator online';
            el.apiStatusText.textContent = 'Backend API: Sẵn sàng (Port 5000)';
            el.apiStatusBadge.title = 'Backend ASP.NET Core đang phản hồi tốt';
            return true;
        } else {
            throw new Error(`Status ${res.status}`);
        }
    } catch {
        el.statusDot.className = 'status-indicator offline';
        el.apiStatusText.textContent = 'Backend API: Chưa kết nối (Bật: dotnet run)';
        el.apiStatusBadge.title = `Không kết nối được tới ${API_BASE}. Chạy lệnh: dotnet run --project backend/WebXuLyAnh.Api.csproj`;
        return false;
    }
}

// ============================================================================
// 4. QUẢN LÝ MA TRẬN ĐẦU VÀO, PADDING 0 & MOBILE RESPONSIVENESS
// ============================================================================

/**
 * Lấy giá trị của ma trận người dùng (chỉ đọc các ô input.c-data bên trong, bỏ qua padding)
 */
function getMatrixValues() {
    const matrix = [];
    for (let r = 0; r < currentRows; r++) {
        const row = [];
        for (let c = 0; c < currentCols; c++) {
            const input = el.mat.querySelector(`input.c-data[data-r="${r}"][data-c="${c}"]`);
            const val = input ? input.value.trim() : '';
            row.push(val);
        }
        matrix.push(row);
    }
    return matrix;
}

/**
 * Render ma trận đầu vào kèm tính năng Ẩn/Hiện Padding 0 & Tối ưu cho Mobile
 */
function renderInputMatrix(oldValues = null) {
    el.mat.innerHTML = '';

    const pad = showPadding ? Math.floor(currentK / 2) : 0;
    const totalRows = currentRows + 2 * pad;
    const totalCols = currentCols + 2 * pad;

    const grid = document.createElement('div');
    grid.className = 'grid';
    grid.style.setProperty('--c', totalCols);

    // Tự động co giãn kích thước ô cho vừa màn hình điện thoại khi số cột lớn
    if (totalCols >= 9) {
        grid.style.setProperty('--cell-size', '28px');
        grid.style.setProperty('--cell-max', '40px');
        grid.style.setProperty('--cell-height', '36px');
        grid.style.setProperty('--cell-gap', '3px');
    } else if (totalCols >= 7) {
        grid.style.setProperty('--cell-size', '32px');
        grid.style.setProperty('--cell-max', '48px');
        grid.style.setProperty('--cell-height', '40px');
        grid.style.setProperty('--cell-gap', '4px');
    }

    for (let pr = 0; pr < totalRows; pr++) {
        for (let pc = 0; pc < totalCols; pc++) {
            const isPad = (pr < pad) || (pr >= pad + currentRows) || (pc < pad) || (pc >= pad + currentCols);

            if (isPad) {
                // Ô PADDING 0 (Chạm/Click để chọn và giữ cửa sổ liên quan ngay lần đầu)
                const padSpan = document.createElement('span');
                padSpan.className = 'c pad';
                padSpan.dataset.pr = pr;
                padSpan.dataset.pc = pc;
                padSpan.dataset.pad = 'true';
                padSpan.textContent = '0';
                padSpan.title = `Chạm để giữ cửa sổ ${currentK}×${currentK} quanh ô này`;

                padSpan.addEventListener('pointerdown', (e) => {
                    e.stopPropagation();
                    selectAndHighlightWindow(pr, pc);
                });
                padSpan.addEventListener('click', (e) => {
                    e.stopPropagation();
                    selectAndHighlightWindow(pr, pc);
                });

                grid.appendChild(padSpan);
            } else {
                // Ô DỮ LIỆU MA TRẬN CỦA NGƯỜI DÙNG
                const r = pr - pad;
                const c = pc - pad;

                const input = document.createElement('input');
                input.type = 'number';
                input.min = '0';
                input.max = '255';
                input.className = 'c c-data';
                input.dataset.r = r;
                input.dataset.c = c;
                input.dataset.pr = pr;
                input.dataset.pc = pc;
                input.dataset.pad = 'false';
                input.placeholder = `${r + 1},${c + 1}`;

                // Phục hồi giá trị cũ
                if (oldValues && oldValues[r] && oldValues[r][c] !== undefined && oldValues[r][c] !== '') {
                    input.value = oldValues[r][c];
                }

                // Điều hướng bàn phím
                input.addEventListener('keydown', handleMatrixKeyNavigation);

                // Chọn và giữ nguyên cửa sổ ngay từ lần ấn đầu tiên (hỗ trợ cả touch, click, focus)
                input.addEventListener('pointerdown', () => {
                    selectAndHighlightWindow(pr, pc);
                });
                input.addEventListener('focus', () => {
                    selectAndHighlightWindow(pr, pc);
                });
                input.addEventListener('click', (e) => {
                    e.stopPropagation();
                    selectAndHighlightWindow(pr, pc);
                });

                grid.appendChild(input);
            }
        }
    }
    el.mat.appendChild(grid);

    // Cập nhật chú thích padding ngắn gọn
    if (el.padHint) {
        if (!showPadding) {
            el.padHint.innerHTML = `💡 Đang <b>TẮT</b> hiển thị Padding 0 (chỉ hiển thị ${currentRows}×${currentCols} ô dữ liệu). Bấm nút trên để BẬT lại nếu cần.`;
        } else if (pad > 0) {
            el.padHint.innerHTML = `💡 Đang <b>BẬT</b> Padding 0 (${pad} vòng số 0 ứng với cửa sổ ${currentK}×${currentK}). Chạm vào bất kỳ ô nào để xem các ô liên quan.`;
        } else {
            el.padHint.innerHTML = `💡 Cửa sổ 1×1: Không cần padding ngoài biên.`;
        }
    }

    // Giữ lại ô đang chọn sau khi re-render lưới (ví dụ khi tăng/giảm k)
    if (selectedWindowCoords) {
        const keepPr = selectedWindowCoords.r !== undefined ? selectedWindowCoords.r + pad : selectedWindowCoords.pr;
        const keepPc = selectedWindowCoords.c !== undefined ? selectedWindowCoords.c + pad : selectedWindowCoords.pc;
        setTimeout(() => selectAndHighlightWindow(keepPr, keepPc), 0);
    }
}

// Biến lưu trữ tọa độ ô đang được click chọn và giữ lại
let selectedWindowCoords = null;

/**
 * Cấp độ 1: Chọn vào từng ô để làm nổi bật tâm và toàn bộ các ô liên quan trong cửa sổ k×k
 * ẤN LẦN ĐẦU ĂN NGAY 100%, KHÓA VÀ GIỮ NGUYÊN VÙNG SÁNG
 */
function selectAndHighlightWindow(centerPr, centerPc) {
    clearSlidingWindowHighlight(false);

    const centerCell = el.mat.querySelector(`[data-pr="${centerPr}"][data-pc="${centerPc}"]`);
    if (!centerCell) return;

    // Lưu lại tọa độ ô đang chọn
    if (centerCell.dataset.pad === 'false') {
        selectedWindowCoords = {
            pr: centerPr,
            pc: centerPc,
            r: parseInt(centerCell.dataset.r, 10),
            c: parseInt(centerCell.dataset.c, 10)
        };
    } else {
        selectedWindowCoords = { pr: centerPr, pc: centerPc };
    }

    // Đánh dấu ô tâm
    centerCell.classList.add('window-center');

    // Đánh dấu toàn bộ các ô liên quan trong cửa sổ k×k
    const half = Math.floor(currentK / 2);
    for (let dr = -half; dr <= half; dr++) {
        for (let dc = -half; dc <= half; dc++) {
            if (dr === 0 && dc === 0) continue;
            const targetPr = centerPr + dr;
            const targetPc = centerPc + dc;
            const cell = el.mat.querySelector(`[data-pr="${targetPr}"][data-pc="${targetPc}"]`);
            if (cell) {
                cell.classList.add('window-active');
            }
        }
    }

    // Hiển thị thông báo vị trí ô đang soi kèm nút đóng ✕
    if (el.selectedWindowInfo) {
        let label = '';
        if (centerCell.dataset.pad === 'true') {
            label = `📍 Tâm: <b>Ô Padding 0 ngoài biên</b> • Cửa sổ ${currentK}×${currentK} bao phủ các ô sáng tím`;
        } else {
            const r = parseInt(centerCell.dataset.r, 10);
            const c = parseInt(centerCell.dataset.c, 10);
            label = `📍 Tâm: <b>Ô điểm ảnh (Hàng ${r + 1}, Cột ${c + 1})</b> • Cửa sổ ${currentK}×${currentK} bao phủ các ô sáng tím`;
        }
        el.selectedWindowInfo.innerHTML = `${label} <button type="button" class="close-badge-btn" id="btnCloseWindowInfo" title="Tắt xem cửa sổ" aria-label="Đóng">✕</button>`;
        el.selectedWindowInfo.hidden = false;

        const closeBtn = document.getElementById('btnCloseWindowInfo');
        if (closeBtn) {
            closeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                clearSlidingWindowHighlight(true);
            });
        }
    }
}

function clearSlidingWindowHighlight(resetCoords = true) {
    const activeCells = el.mat.querySelectorAll('.window-active');
    activeCells.forEach(c => c.classList.remove('window-active'));

    const centerCells = el.mat.querySelectorAll('.window-center');
    centerCells.forEach(c => c.classList.remove('window-center'));

    if (resetCoords) {
        selectedWindowCoords = null;
        if (el.selectedWindowInfo) {
            el.selectedWindowInfo.hidden = true;
            el.selectedWindowInfo.innerHTML = '';
        }
    }
}

/**
 * Điều hướng mũi tên và Enter giữa các ô dữ liệu
 */
function handleMatrixKeyNavigation(e) {
    const r = parseInt(this.dataset.r, 10);
    const c = parseInt(this.dataset.c, 10);
    let targetR = r;
    let targetC = c;

    switch (e.key) {
        case 'ArrowUp':
            targetR = Math.max(0, r - 1);
            break;
        case 'ArrowDown':
            targetR = Math.min(currentRows - 1, r + 1);
            break;
        case 'ArrowLeft':
            if (this.selectionStart === 0) {
                targetC = Math.max(0, c - 1);
            } else return;
            break;
        case 'ArrowRight':
            if (this.selectionEnd === this.value.length) {
                targetC = Math.min(currentCols - 1, c + 1);
            } else return;
            break;
        case 'Enter':
            e.preventDefault();
            if (c + 1 < currentCols) {
                targetC = c + 1;
            } else if (r + 1 < currentRows) {
                targetR = r + 1;
                targetC = 0;
            }
            break;
        default:
            return;
    }

    if (targetR !== r || targetC !== c) {
        const nextInput = el.mat.querySelector(`input.c-data[data-r="${targetR}"][data-c="${targetC}"]`);
        if (nextInput) {
            nextInput.focus();
            nextInput.select();
        }
    }
}

// Cập nhật số hàng / cột
function updateMatrixDimensions(newRows, newCols) {
    const clampedRows = Math.min(20, Math.max(1, newRows));
    const clampedCols = Math.min(20, Math.max(1, newCols));

    if (clampedRows === currentRows && clampedCols === currentCols) return;

    const oldVals = getMatrixValues();
    currentRows = clampedRows;
    currentCols = clampedCols;

    el.rows.value = currentRows;
    el.cols.value = currentCols;

    renderInputMatrix(oldVals);
}

// Cập nhật kích thước cửa sổ k
function updateK(newK) {
    const clampedK = Math.min(9, Math.max(1, newK));
    if (clampedK === currentK) return;

    const oldVals = getMatrixValues();
    currentK = clampedK;
    el.k.value = currentK;

    el.quickKChips.forEach(chip => {
        const chipK = parseInt(chip.dataset.k, 10);
        chip.classList.toggle('active', chipK === currentK);
    });

    if (currentMethod === 'prewitt') renderPrewittKernels();
    renderInputMatrix(oldVals);
}

// ============================================================================
// 5. CÔNG CỤ ĐIỀN GIÁ TRỊ VÀ THAO TÁC MA TRẬN
// ============================================================================

/**
 * Điền toàn bộ các ô trong ma trận bằng một số cụ thể
 */
function fillAllWith(value) {
    const num = parseInt(value, 10);
    if (isNaN(num) || num < 0 || num > 255) {
        showError('Vui lòng nhập số nguyên hợp lệ trong khoảng [0, 255].');
        return;
    }

    const inputs = el.mat.querySelectorAll('input.c-data');
    inputs.forEach(inp => {
        inp.value = num;
        inp.style.borderColor = '';
    });
    clearError();
}

/**
 * Dùng ma trận mẫu chuẩn giáo trình
 */
function fillSampleMatrix() {
    const sample = [
        [10, 20, 30],
        [40, 50, 60],
        [70, 80, 90]
    ];
    updateMatrixDimensions(3, 3);
    sample.forEach((row, r) => {
        row.forEach((val, c) => {
            const inp = el.mat.querySelector(`input.c-data[data-r="${r}"][data-c="${c}"]`);
            if (inp) inp.value = val;
        });
    });
    clearError();
}

function clearMatrix() {
    const inputs = el.mat.querySelectorAll('input.c-data');
    inputs.forEach(inp => {
        inp.value = '';
        inp.style.borderColor = '';
    });
    clearError();
}

// Xử lý dán dữ liệu từ Excel / Clipboard
function parseAndFillPastedData(text) {
    if (!text || !text.trim()) return;

    const lines = text.trim().split(/\r?\n/).filter(line => line.trim().length > 0);
    if (lines.length === 0) return;

    const matrix = lines.map(line => {
        if (line.includes('\t')) return line.split('\t').map(v => v.trim());
        if (line.includes(',')) return line.split(',').map(v => v.trim());
        return line.trim().split(/\s+/).map(v => v.trim());
    });

    const rows = Math.min(20, matrix.length);
    const cols = Math.min(20, Math.max(...matrix.map(r => r.length)));

    updateMatrixDimensions(rows, cols);

    for (let r = 0; r < rows; r++) {
        for (let c = 0; c < cols; c++) {
            const inp = el.mat.querySelector(`input.c-data[data-r="${r}"][data-c="${c}"]`);
            if (inp && matrix[r] && matrix[r][c] !== undefined) {
                const num = parseInt(matrix[r][c], 10);
                inp.value = isNaN(num) ? '' : Math.min(255, Math.max(0, num));
            }
        }
    }
}

// ============================================================================
// 6. QUẢN LÝ PHƯƠNG PHÁP & KERNEL PREWITT
// ============================================================================
function setMethod(method) {
    currentMethod = method.toLowerCase();
    el.methodCards.forEach(card => {
        const isMatch = card.dataset.m === currentMethod;
        card.setAttribute('aria-pressed', isMatch ? 'true' : 'false');
    });

    if (currentMethod === 'prewitt') {
        el.kersSection.hidden = false;
        renderPrewittKernels();
    } else {
        el.kersSection.hidden = true;
    }
}

function renderPrewittKernels() {
    const k = currentK;
    el.gxContainer.innerHTML = '';
    el.gyContainer.innerHTML = '';

    const gridGx = document.createElement('div');
    gridGx.className = 'grid';
    gridGx.style.setProperty('--c', k);

    const gridGy = document.createElement('div');
    gridGy.className = 'grid';
    gridGy.style.setProperty('--c', k);

    const defaultGx = [
        [-1, 0, 1],
        [-1, 0, 1],
        [-1, 0, 1]
    ];
    const defaultGy = [
        [-1, -1, -1],
        [0, 0, 0],
        [1, 1, 1]
    ];

    for (let r = 0; r < k; r++) {
        for (let c = 0; c < k; c++) {
            const inx = document.createElement('input');
            inx.type = 'number';
            inx.dataset.r = r;
            inx.dataset.c = c;
            inx.value = (k === 3) ? defaultGx[r][c] : 0;
            gridGx.appendChild(inx);

            const iny = document.createElement('input');
            iny.type = 'number';
            iny.dataset.r = r;
            iny.dataset.c = c;
            iny.value = (k === 3) ? defaultGy[r][c] : 0;
            gridGy.appendChild(iny);
        }
    }

    el.gxContainer.appendChild(gridGx);
    el.gyContainer.appendChild(gridGy);
}

function getKernelData(container) {
    const inputs = container.querySelectorAll('input');
    const kernel = [];
    const k = currentK;
    for (let r = 0; r < k; r++) {
        const row = [];
        for (let c = 0; c < k; c++) {
            const val = parseInt(inputs[r * k + c].value, 10);
            if (isNaN(val)) return null;
            row.push(val);
        }
        kernel.push(row);
    }
    return kernel;
}

// ============================================================================
// 7. VALIDATION DỮ LIỆU ĐẦU VÀO
// ============================================================================
function showError(msg) {
    el.errBox.textContent = msg;
    el.errBox.style.display = 'block';
}

function clearError() {
    el.errBox.textContent = '';
    el.errBox.style.display = 'none';
}

function validateInputs() {
    clearError();
    const inputs = el.mat.querySelectorAll('input.c-data');
    const matrix = [];
    let hasEmpty = false;
    let outOfRange = false;

    inputs.forEach(inp => inp.style.borderColor = '');

    for (let r = 0; r < currentRows; r++) {
        const row = [];
        for (let c = 0; c < currentCols; c++) {
            const inp = el.mat.querySelector(`input.c-data[data-r="${r}"][data-c="${c}"]`);
            if (!inp) continue;
            const raw = inp.value.trim();

            if (raw === '') {
                hasEmpty = true;
                inp.style.borderColor = 'var(--err)';
            } else {
                const num = parseInt(raw, 10);
                if (isNaN(num) || num < 0 || num > 255) {
                    outOfRange = true;
                    inp.style.borderColor = 'var(--err)';
                } else {
                    row.push(num);
                }
            }
        }
        matrix.push(row);
    }

    if (hasEmpty) {
        showError('Lỗi: Ma trận chưa được điền đủ (hãy điền từng ô hoặc dùng nút "Điền tất cả ô").');
        return null;
    }
    if (outOfRange) {
        showError('Lỗi: Giá trị từng điểm ảnh phải là số nguyên trong khoảng [0, 255].');
        return null;
    }

    let prewittConfig = null;
    if (currentMethod === 'prewitt') {
        const kx = getKernelData(el.gxContainer);
        const ky = getKernelData(el.gyContainer);
        if (!kx || !ky) {
            showError('Lỗi: Các ô trong Kernel Gx và Gy phải là số nguyên hợp lệ.');
            return null;
        }
        prewittConfig = { kernelX: kx, kernelY: ky };
    }

    return {
        matrix,
        k: currentK,
        method: currentMethod.toUpperCase(),
        prewittConfig,
        options: { includeSteps: true }
    };
}

// ============================================================================
// 8. GỬI REQUEST LÊN BACKEND & XỬ LÝ KẾT QUẢ (CHỈ KHI BẤM NÚT)
// ============================================================================
async function executeFilter() {
    const payload = validateInputs();
    if (!payload) {
        el.runSpinner.classList.remove('active');
        return;
    }

    el.runBtn.disabled = true;
    el.runSpinner.classList.add('active');
    el.runBtnText.textContent = 'Đang tính toán...';

    try {
        const res = await fetch(`${API_BASE}/api/filter`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (res.status === 422) {
            const errorData = await res.json();
            const details = errorData.errors ? errorData.errors.map(e => e.message).join(' | ') : errorData.errorSummary;
            showError(`Dữ liệu không hợp lệ (422): ${details}`);
            return;
        }

        if (!res.ok) {
            throw new Error(`Mã lỗi HTTP: ${res.status} (${res.statusText})`);
        }

        const data = await res.json();
        lastResponseData = data;

        renderResults();
        el.outSection.hidden = false;
        el.outSection.scrollIntoView({ behavior: 'smooth', block: 'start' });

    } catch (err) {
        console.error('API Error:', err);
        showError(`Không thể kết nối đến Backend tại ${API_BASE}.\nChi tiết: ${err.message}\n👉 Hãy mở terminal và chạy: dotnet run --project backend/WebXuLyAnh.Api.csproj`);
    } finally {
        el.runBtn.disabled = false;
        el.runSpinner.classList.remove('active');
        el.runBtnText.textContent = 'Tính kết quả';
        checkApiHealth();
    }
}

// ============================================================================
// 9. HIỂN THỊ KẾT QUẢ & PHÂN CHIA MÀU SẮC HEATMAP
// ============================================================================
function renderResults() {
    if (!lastResponseData || !lastResponseData.results) return;

    // Cảnh báo từ backend (nếu có)
    el.warningsBox.innerHTML = '';
    if (lastResponseData.warnings && lastResponseData.warnings.length > 0) {
        el.warningsBox.hidden = false;
        lastResponseData.warnings.forEach(w => {
            const p = document.createElement('div');
            p.innerHTML = `⚠️ <b>${w.code}</b>: ${w.message}`;
            el.warningsBox.appendChild(p);
        });
    } else {
        el.warningsBox.hidden = true;
    }

    // Render lưới kết quả
    el.resContainer.innerHTML = '';
    const grid = document.createElement('div');
    grid.className = 'grid final';
    grid.style.setProperty('--c', lastResponseData.matrixSize.cols);

    lastResponseData.results.forEach((row, r) => {
        row.forEach((cell, c) => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'c';
            btn.dataset.r = r;
            btn.dataset.c = c;
            btn.title = `Nhấn để xem chi tiết tính toán ô (${r + 1}, ${c + 1})`;

            if (r === selectedCellCoords.r && c === selectedCellCoords.c) {
                btn.classList.add('active');
            }

            btn.textContent = getCellDisplayValue(cell, currentFormat);

            btn.addEventListener('click', () => {
                selectResultCell(r, c);
            });

            grid.appendChild(btn);
        });
    });

    el.resContainer.appendChild(grid);

    // Áp dụng màu Heatmap cho các số giống nhau
    applyHeatmapColors();

    renderStepDetail(selectedCellCoords.r, selectedCellCoords.c);
}

/**
 * Phân chia màu sắc thông minh cho các ô trong ma trận kết quả:
 * Các ô có số giống nhau sẽ cùng chung một mã màu nền 100%!
 */
function applyHeatmapColors() {
    if (!lastResponseData || !lastResponseData.results) return;

    const buttons = el.resContainer.querySelectorAll('button.c');
    if (!buttons || buttons.length === 0) return;

    if (!enableHeatmap) {
        buttons.forEach(btn => {
            btn.style.backgroundColor = '';
            btn.style.color = '';
            btn.style.borderColor = '';
            btn.style.boxShadow = '';
        });
        return;
    }

    // Lấy danh sách giá trị số để tìm min và max
    const numValues = [];
    lastResponseData.results.forEach(row => {
        row.forEach(cell => {
            let val;
            if (currentFormat === 'roundedInt') {
                val = cell.roundedInt;
            } else if (currentFormat === 'rounded1') {
                val = parseFloat(cell.rounded1) || 0;
            } else {
                val = cell.exact.denominator !== 0 ? (cell.exact.numerator / cell.exact.denominator) : 0;
            }
            numValues.push(val);
        });
    });

    const minVal = Math.min(...numValues);
    const maxVal = Math.max(...numValues);
    const range = maxVal - minVal;

    lastResponseData.results.forEach((row, r) => {
        row.forEach((cell, c) => {
            const idx = r * lastResponseData.matrixSize.cols + c;
            const btn = buttons[idx];
            if (!btn) return;

            let val;
            if (currentFormat === 'roundedInt') {
                val = cell.roundedInt;
            } else if (currentFormat === 'rounded1') {
                val = parseFloat(cell.rounded1) || 0;
            } else {
                val = cell.exact.denominator !== 0 ? (cell.exact.numerator / cell.exact.denominator) : 0;
            }

            const t = range > 0 ? (val - minVal) / range : 0.5;
            // Tông màu Indigo dịu mắt: alpha từ 0.08 đến 0.76
            const alpha = 0.08 + t * 0.68;
            btn.style.backgroundColor = `rgba(99, 102, 241, ${alpha.toFixed(2)})`;
            if (alpha > 0.42) {
                btn.style.color = '#ffffff';
                btn.style.borderColor = 'rgba(79, 70, 229, 0.9)';
            } else {
                btn.style.color = 'var(--ink)';
                btn.style.borderColor = 'rgba(99, 102, 241, 0.25)';
            }
        });
    });
}

function getCellDisplayValue(cell, format) {
    switch (format) {
        case 'rounded1':
            return cell.rounded1;
        case 'exact':
            return cell.exact.display;
        case 'roundedInt':
        default:
            return cell.roundedInt.toString();
    }
}

function updateDisplayFormat(format) {
    currentFormat = format;
    el.formatToggleBtns.forEach(b => {
        b.classList.toggle('active', b.dataset.fmt === format);
    });

    if (!lastResponseData || !lastResponseData.results) return;

    const buttons = el.resContainer.querySelectorAll('button.c');
    lastResponseData.results.forEach((row, r) => {
        row.forEach((cell, c) => {
            const idx = r * lastResponseData.matrixSize.cols + c;
            if (buttons[idx]) {
                buttons[idx].textContent = getCellDisplayValue(cell, currentFormat);
            }
        });
    });

    applyHeatmapColors();
    renderStepDetail(selectedCellCoords.r, selectedCellCoords.c);
}

function selectResultCell(r, c) {
    selectedCellCoords = { r, c };

    const buttons = el.resContainer.querySelectorAll('button.c');
    buttons.forEach(btn => {
        const isMatch = parseInt(btn.dataset.r, 10) === r && parseInt(btn.dataset.c, 10) === c;
        btn.classList.toggle('active', isMatch);
    });

    renderStepDetail(r, c);
}

// ============================================================================
// 10. HIỂN THỊ CHI TIẾT TỪNG BƯỚC TÍNH TOÁN
// ============================================================================
function renderStepDetail(r, c) {
    if (!lastResponseData || !lastResponseData.results) return;
    const cell = lastResponseData.results[r]?.[c];
    if (!cell || !cell.step) return;

    el.detailSection.hidden = false;
    const step = cell.step;
    const k = lastResponseData.k;

    let html = `
        <div class="calc-section-title">
            <span>📍 Tọa độ ô: <b>${cell.displayCoordinates}</b> (Hàng ${r + 1}, Cột ${c + 1})</span>
        </div>
        <div style="display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 14px;">
            <span class="calc-tag">Số nguyên: <b>${cell.roundedInt}</b></span>
            <span class="calc-tag">Làm tròn 1 số: <b>${cell.rounded1}</b></span>
            <span class="calc-tag">Phân số chính xác: <b>${cell.exact.display}</b></span>
        </div>
    `;

    html += `
        <div class="calc-section-title">Cửa sổ trượt ${k}×${k} (Window):</div>
        <div class="calc-grid-wrap">
            <div>
                <div class="grid" style="--c: ${k}">
    `;

    const padTop = lastResponseData.padding.padTop;
    const padLeft = lastResponseData.padding.padLeft;
    const centerR = Math.floor(k / 2);
    const centerC = Math.floor(k / 2);

    step.window.forEach((wRow, wr) => {
        wRow.forEach((val, wc) => {
            const origR = r - padTop + wr;
            const origC = c - padLeft + wc;
            const isPad = origR < 0 || origR >= lastResponseData.matrixSize.rows || origC < 0 || origC >= lastResponseData.matrixSize.cols;
            const isCenter = wr === centerR && wc === centerC;

            let classes = 'c';
            if (isPad) classes += ' pad';
            if (isCenter) classes += ' mid';

            html += `<span class="${classes}" title="${isCenter ? 'Tâm cửa sổ' : (isPad ? 'Padding 0' : 'Điểm ảnh')}">${val}</span>`;
        });
    });

    html += `
                </div>
                <p class="hint" style="margin-top:6px;">Ô viền đậm: Tâm cửa sổ. Ô nét đứt: Giá trị Padding 0.</p>
            </div>
            <div style="flex: 1; min-width: 250px;">
    `;

    if (step.mean) {
        html += `
            <div class="calc-section-title">Công thức Lọc Trung Bình (Mean):</div>
            <div class="calc-formula-card">
                <div>${step.mean.formula}</div>
                <div style="margin-top: 6px;">→ Tổng các ô = <b>${step.mean.sum}</b> / ${step.mean.count}</div>
                <div>→ Phân số tối giản: <b>${step.mean.reducedFraction}</b></div>
                <div>→ Số nguyên: <b>${cell.roundedInt}</b> (Làm tròn 1 số: <b>${cell.rounded1}</b>)</div>
            </div>
        `;
    } else if (step.median) {
        html += `
            <div class="calc-section-title">Công thức Lọc Trung Vị (Median):</div>
            <div class="calc-formula-card">
                <div>Các phần tử trong cửa sổ: [${step.median.rawElements.join(', ')}]</div>
                <div style="margin-top: 6px;">Sau khi sắp xếp tăng dần:</div>
                <div style="font-weight: 700; color: var(--a1);">[${step.median.sortedElements.join(', ')}]</div>
                <div style="margin-top: 6px;">${step.median.formula}</div>
                <div>→ Giá trị trung vị: <b>${cell.rounded1}</b> (Nguyên: <b>${cell.roundedInt}</b>)</div>
            </div>
        `;
    } else if (step.prewitt) {
        html += `
            <div class="calc-section-title">Công thức Phát hiện biên (Prewitt Edge Detection):</div>
            <div class="calc-formula-card">
                <div style="color: var(--muted); font-size: 0.8rem; margin-bottom: 4px;">* Quy ước tích chập: Kernel lật 180° trước khi nhân với cửa sổ.</div>
                <div><b>Gx (Biên dọc):</b></div>
                <div>${step.prewitt.formulaGx}</div>
                <div>→ Gx = <b>${step.prewitt.gx}</b> (|Gx| = <b>${step.prewitt.absGx}</b>)</div>
                <div style="margin-top: 8px;"><b>Gy (Biên ngang):</b></div>
                <div>${step.prewitt.formulaGy}</div>
                <div>→ Gy = <b>${step.prewitt.gy}</b> (|Gy| = <b>${step.prewitt.absGy}</b>)</div>
                <div style="margin-top: 8px; border-top: 1px dashed var(--line); padding-top: 6px;">
                    <b>Độ lớn gradient biên (G):</b><br>
                    ${step.prewitt.formulaG} = <b>${step.prewitt.g}</b>
                </div>
            </div>
        `;
    }

    html += `
            </div>
        </div>
    `;

    el.detailSection.innerHTML = html;
}

// ============================================================================
// 11. GẮN SỰ KIỆN KHỞI CHẠY (EVENT LISTENERS)
// ============================================================================
function initEventListeners() {
    // Steppers hàng, cột
    el.btnDecRows.addEventListener('click', () => updateMatrixDimensions(currentRows - 1, currentCols));
    el.btnIncRows.addEventListener('click', () => updateMatrixDimensions(currentRows + 1, currentCols));
    el.btnDecCols.addEventListener('click', () => updateMatrixDimensions(currentRows, currentCols - 1));
    el.btnIncCols.addEventListener('click', () => updateMatrixDimensions(currentRows, currentCols + 1));

    el.rows.addEventListener('change', () => updateMatrixDimensions(parseInt(el.rows.value, 10), currentCols));
    el.cols.addEventListener('change', () => updateMatrixDimensions(currentRows, parseInt(el.cols.value, 10)));

    // Steppers & Chips cửa sổ k (Chỉ 3x3, 5x5, 7x7)
    el.btnDecK.addEventListener('click', () => updateK(currentK - 1));
    el.btnIncK.addEventListener('click', () => updateK(currentK + 1));
    el.k.addEventListener('change', () => {
        const val = parseInt(el.k.value, 10);
        if (!isNaN(val)) updateK(val);
    });

    el.quickKChips.forEach(chip => {
        chip.addEventListener('click', () => {
            const kVal = parseInt(chip.dataset.k, 10);
            updateK(kVal);
        });
    });

    // Điền tất cả ô
    el.btnFillAll.addEventListener('click', () => {
        const val = el.fillVal.value.trim();
        if (val === '') {
            showError('Vui lòng nhập số trong khoảng [0, 255] vào ô để điền tất cả.');
            return;
        }
        fillAllWith(val);
    });

    // Bật / Tắt hiển thị Padding 0
    el.btnTogglePad.addEventListener('click', () => {
        showPadding = !showPadding;
        el.padToggleIcon.textContent = showPadding ? '👁️' : '👁️‍🗨️';
        el.padToggleText.textContent = showPadding ? 'Padding 0: BẬT' : 'Padding 0: TẮT';
        el.btnTogglePad.setAttribute('aria-pressed', showPadding ? 'true' : 'false');
        renderInputMatrix(getMatrixValues());
    });

    // Bật / Tắt Tô màu Heatmap
    el.btnToggleHeatmap.addEventListener('click', () => {
        enableHeatmap = !enableHeatmap;
        el.btnToggleHeatmap.classList.toggle('active', enableHeatmap);
        el.heatmapText.textContent = enableHeatmap ? 'Màu trực quan: BẬT' : 'Màu trực quan: TẮT';
        applyHeatmapColors();
    });

    // Thao tác ma trận
    el.sampleBtn.addEventListener('click', fillSampleMatrix);
    el.clearBtn.addEventListener('click', clearMatrix);

    // Dán từ Excel qua nút hoặc Ctrl+V
    el.pasteExcelBtn.addEventListener('click', async () => {
        try {
            if (navigator.clipboard && navigator.clipboard.readText) {
                const text = await navigator.clipboard.readText();
                if (text && text.trim()) {
                    parseAndFillPastedData(text);
                    return;
                }
            }
        } catch {
            // Clipboard bị chặn
        }
        const input = prompt('Dán (Ctrl+V) ma trận từ Excel hoặc văn bản vào đây:');
        if (input) parseAndFillPastedData(input);
    });

    document.addEventListener('paste', (e) => {
        const target = e.target;
        if (target && target.tagName === 'INPUT' && el.mat.contains(target)) {
            const pasteData = (e.clipboardData || window.clipboardData).getData('text');
            if (pasteData && (pasteData.includes('\t') || pasteData.includes('\n'))) {
                e.preventDefault();
                parseAndFillPastedData(pasteData);
            }
        }
    });

    // Chọn phương pháp
    el.methodCards.forEach(card => {
        card.addEventListener('click', () => {
            setMethod(card.dataset.m);
        });
    });

    // Đặt lại kernel Prewitt
    el.resetKernelBtn.addEventListener('click', renderPrewittKernels);

    // Nút Tính kết quả (CHỈ KHI BẤM MỚI CHẠY)
    el.runBtn.addEventListener('click', executeFilter);

    // Quick toggle định dạng
    el.formatToggleBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            updateDisplayFormat(btn.dataset.fmt);
        });
    });

    // Chạm/Click ra ngoài ma trận để xóa highlight
    document.addEventListener('click', (e) => {
        if (!el.mat.contains(e.target) && e.target !== el.btnTogglePad) {
            clearSlidingWindowHighlight();
        }
    });
}

// Khởi tạo trang web
window.addEventListener('DOMContentLoaded', () => {
    el.runSpinner.classList.remove('active');

    // Mặc định: ma trận 3x3 và TRỐNG
    renderInputMatrix();
    setMethod('mean');
    initEventListeners();
    checkApiHealth();
    setInterval(checkApiHealth, 10000);
});
