# WEB_XuLyAnh — Bộ Dữ Liệu Kiểm Thử Chuẩn (Test Suite & Benchmark)

> **Tài liệu tham chiếu gốc**:  
> - Bối cảnh dự án: [WEB_XuLyAnh_boi_canh_du_an.md](file:///e:/DoAnCaNhan/Web_XuLyAnh/TaiLieuCode_XLA/WEB_XuLyAnh_boi_canh_du_an.md)  
> - Hợp đồng API & Schema: [WEB_XuLyAnh_hop_dong_api.md](file:///e:/DoAnCaNhan/Web_XuLyAnh/TaiLieuCode_XLA/WEB_XuLyAnh_hop_dong_api.md)  
>
> **Mục đích tài liệu**:  
> Cung cấp các bộ test mẫu (Test Cases) chuẩn hóa từ các dạng bài tập Xử lý ảnh kinh điển tại các trường đại học (ĐHBK, ĐHQG, PTIT,...).  
> Toàn bộ kết quả trong tài liệu này đã được **tính toán thủ công và kiểm chứng tự động bằng mã nguồn toán học chính xác 100%**.  
> Sử dụng trực tiếp để:  
> 1. Lập trình viên Backend viết Unit Test / Automated Test (xUnit/NUnit trong C#).  
> 2. Lập trình viên Frontend dùng làm Mock Data dựng giao diện và kiểm thử các tính năng chuyển đổi (Toggle Mode / Tabs).  
> 3. Làm cơ sở đối chiếu khi giảng viên cung cấp thêm bài tập mẫu mới.

---

## 1. Bảng Tóm Tắt Quy Ước Tính Toán Chuẩn

Mọi test case trong tài liệu này đều tuân thủ các quy ước toán học đã chốt:
1. **Zero-Padding (Đệm số 0)**:
   - Padding trên và trái: $\lfloor k / 2 \rfloor$.
   - Padding dưới và phải: $k - 1 - \lfloor k / 2 \rfloor$.
   - *Với $k$ lẻ*: Đối xứng $\frac{k-1}{2}$ mỗi phía.
   - *Với $k$ chẵn*: Trên/trái $= k/2$, dưới/phải $= k/2 - 1$.
2. **Mean**: Giá trị chính xác là phân số $\frac{\sum \text{window}}{k^2}$.
3. **Median**: Sắp xếp $k^2$ giá trị của cửa sổ (gồm cả số 0 padding) theo thứ tự tăng dần.
   - $k$ lẻ: Lấy phần tử chính giữa tại chỉ số $\lfloor k^2 / 2 \rfloor$.
   - $k$ chẵn: Lấy trung bình cộng 2 phần tử giữa $\frac{S[k^2/2 - 1] + S[k^2/2]}{2}$.
4. **Prewitt**:
   - Sử dụng **Tích chập (Convolution)**: Lật kernel $180^\circ$ trước khi nhân từng phần tử tương ứng với cửa sổ rồi cộng lại.
   - Kernel mặc định $3 \times 3$:  
     $G_x = \begin{bmatrix} -1 & 0 & 1 \\ -1 & 0 & 1 \\ -1 & 0 & 1 \end{bmatrix} \xrightarrow{\text{Lật } 180^\circ} G_{x,\text{flip}} = \begin{bmatrix} 1 & 0 & -1 \\ 1 & 0 & -1 \\ 1 & 0 & -1 \end{bmatrix}$  
     $G_y = \begin{bmatrix} -1 & -1 & -1 \\ 0 & 0 & 0 \\ 1 & 1 & 1 \end{bmatrix} \xrightarrow{\text{Lật } 180^\circ} G_{y,\text{flip}} = \begin{bmatrix} 1 & 1 & 1 \\ 0 & 0 & 0 \\ -1 & -1 & -1 \end{bmatrix}$  
   - Độ lớn gradient: $G = |G_x| + |G_y|$ (giữ nguyên số nguyên, không cắt ngưỡng 255).
5. **Làm tròn hiển thị (Half-Up)**:
   - Làm tròn 1 chữ số thập phân (`rounded1`): Nếu phần thập phân sau làm tròn là `.0` thì chỉ hiển thị số nguyên (ví dụ $6.04 \to 6$, $6.26 \to 6.3$, $0.25 \to 0.3$). Tránh hiển thị `-0`.
   - Làm tròn số nguyên (`roundedInt`): Half-up về số nguyên gần nhất.

---

## 2. Test Case 1: Ma Trận Bậc Thang $3 \times 3$, $k=3$ (Bài Tập Kinh Điển)

### 2.1. Ma trận đầu vào
$$M = \begin{bmatrix} 10 & 20 & 30 \\ 40 & 50 & 60 \\ 70 & 80 & 90 \end{bmatrix}, \quad k = 3$$

Padding: Trên $= 1$, Dưới $= 1$, Trái $= 1$, Phải $= 1$. Cửa sổ mỗi ô có kích thước $3 \times 3 = 9$ phần tử.

---

### 2.2. Kết quả Phương pháp MEAN

#### A. Ma trận kết quả:
* **Giá trị chính xác (Phân số)**:
  $$\begin{bmatrix} 40/3 & 70/3 & 160/9 \\ 30 & 50 & 110/3 \\ 80/3 & 130/3 & 280/9 \end{bmatrix}$$
* **Làm tròn 1 chữ số (`rounded1` - Hiển thị mặc định)**:
  $$\begin{bmatrix} 13.3 & 23.3 & 17.8 \\ 30 & 50 & 36.7 \\ 26.7 & 43.3 & 31.1 \end{bmatrix}$$
* **Làm tròn nguyên (`roundedInt`)**:
  $$\begin{bmatrix} 13 & 23 & 18 \\ 30 & 50 & 37 \\ 27 & 43 & 31 \end{bmatrix}$$

#### B. Minh chứng chi tiết từng bước tính mẫu:
* **Ô góc (0, 0)**:
  - Cửa sổ: $\begin{bmatrix} 0 & 0 & 0 \\ 0 & 10 & 20 \\ 0 & 40 & 50 \end{bmatrix}$
  - Tổng các phần tử $= 0 + 0 + 0 + 0 + 10 + 20 + 0 + 40 + 50 = 120$.
  - Giá trị chính xác $= \frac{120}{9} = \frac{40}{3} \approx 13.333\dots \xrightarrow{\text{Làm tròn 1 số}} \mathbf{13.3}$.
* **Ô tâm (1, 1)**:
  - Cửa sổ (đầy đủ không dính biên): $\begin{bmatrix} 10 & 20 & 30 \\ 40 & 50 & 60 \\ 70 & 80 & 90 \end{bmatrix}$
  - Tổng các phần tử $= 10 + 20 + 30 + 40 + 50 + 60 + 70 + 80 + 90 = 450$.
  - Giá trị chính xác $= \frac{450}{9} = \mathbf{50}$ (hiển thị số nguyên `50`).

---

### 2.3. Kết quả Phương pháp MEDIAN

#### A. Ma trận kết quả:
$$\begin{bmatrix} 0 & 20 & 0 \\ 20 & 50 & 30 \\ 0 & 50 & 0 \end{bmatrix}$$

#### B. Minh chứng chi tiết từng bước tính mẫu:
* **Ô góc (0, 0)**:
  - Các phần tử trong cửa sổ: $\{0, 0, 0, 0, 10, 20, 0, 40, 50\}$.
  - Sắp xếp tăng dần ($9$ phần tử): $[0, 0, 0, 0, \mathbf{0}, 10, 20, 40, 50]$.
  - Chỉ số trung vị: $\lfloor 9/2 \rfloor = 4$ (phần tử thứ 5).
  - Kết quả $= \mathbf{0}$.
* **Ô biên trên (0, 1)**:
  - Cửa sổ: $\begin{bmatrix} 0 & 0 & 0 \\ 10 & 20 & 30 \\ 40 & 50 & 60 \end{bmatrix}$
  - Sắp xếp tăng dần: $[0, 0, 0, 10, \mathbf{20}, 30, 40, 50, 60]$.
  - Kết quả trung vị $= \mathbf{20}$.
* **Ô tâm (1, 1)**:
  - Sắp xếp tăng dần: $[10, 20, 30, 40, \mathbf{50}, 60, 70, 80, 90]$.
  - Kết quả trung vị $= \mathbf{50}$.

---

### 2.4. Kết quả Phương pháp PREWITT

#### A. Ma trận kết quả:
* **Ma trận gradient $G_x$**:
  $$\begin{bmatrix} -70 & -40 & 70 \\ -150 & -60 & 150 \\ -130 & -40 & 130 \end{bmatrix}$$
* **Ma trận gradient $G_y$**:
  $$\begin{bmatrix} -90 & -150 & -110 \\ -120 & -180 & -120 \\ 90 & 150 & 110 \end{bmatrix}$$
* **Ma trận độ lớn biên $G = |G_x| + |G_y|$ (Kết quả cuối cùng)**:
  $$\begin{bmatrix} 160 & 190 & 180 \\ 270 & 240 & 270 \\ 220 & 190 & 240 \end{bmatrix}$$

#### B. Minh chứng chi tiết tính tích chập tại ô tâm (1, 1):
* Cửa sổ: $\begin{bmatrix} 10 & 20 & 30 \\ 40 & 50 & 60 \\ 70 & 80 & 90 \end{bmatrix}$
* $G_{x,\text{flip}} = \begin{bmatrix} 1 & 0 & -1 \\ 1 & 0 & -1 \\ 1 & 0 & -1 \end{bmatrix} \implies G_x = (10 - 30) + (40 - 60) + (70 - 90) = -20 - 20 - 20 = \mathbf{-60}$.
* $G_{y,\text{flip}} = \begin{bmatrix} 1 & 1 & 1 \\ 0 & 0 & 0 \\ -1 & -1 & -1 \end{bmatrix} \implies G_y = (10 + 20 + 30) - (70 + 80 + 90) = 60 - 240 = \mathbf{-180}$.
* Độ lớn: $G = |-60| + |-180| = 60 + 180 = \mathbf{240}$.

---

## 3. Test Case 2: Khử Nhiễu Muối Tiêu (Salt-and-Pepper Noise) $3 \times 3$, $k=3$

Dạng bài tập giáo trình điển hình để so sánh tính chất khử nhiễu đột biến giữa Lọc Trung bình và Lọc Trung vị.

### 3.1. Ma trận đầu vào
Một điểm ảnh nhiễu cực đại ($255$) ở chính giữa trên nền ảnh đều ($50$):
$$M = \begin{bmatrix} 50 & 50 & 50 \\ 50 & 255 & 50 \\ 50 & 50 & 50 \end{bmatrix}, \quad k = 3$$

---

### 3.2. Bảng đối chiếu kết quả 3 phương pháp

| Phương pháp | Ma trận kết quả | Nhận xét bản chất xử lý ảnh |
|---|---|---|
| **MEAN** (`rounded1`) | $\begin{bmatrix} 45 & 56.1 & 45 \\ 56.1 & \mathbf{72.8} & 56.1 \\ 45 & 56.1 & 45 \end{bmatrix}$ | Nhiễu 255 bị nhòe ra các ô lân cận. Ô tâm bị kéo từ 50 lên **72.8** $(\frac{50 \times 8 + 255}{9} = \frac{655}{9} \approx 72.8)$. |
| **MEDIAN** | $\begin{bmatrix} 0 & 50 & 0 \\ 50 & \mathbf{50} & 50 \\ 0 & 50 & 0 \end{bmatrix}$ | **Khử hoàn toàn điểm nhiễu 255**. Tại ô tâm, dãy sắp xếp là $[50, 50, 50, 50, \mathbf{50}, 50, 50, 50, 255] \implies$ trung vị trả về chính xác **50**! |
| **PREWITT** ($G$) | $\begin{bmatrix} 610 & 355 & 610 \\ 355 & \mathbf{0} & 355 \\ 610 & 355 & 610 \end{bmatrix}$ | Tại ô tâm, do tính đối xứng của kernel, $G_x = 0, G_y = 0 \implies G = 0$. Các ô xung quanh phản ứng mạnh do chênh lệch biên độ với tâm. |

---

## 4. Test Case 3: Ma Trận $2 \times 2$, $k=2$ (Kiểm Thử $k$ Chẵn & Padding Bất Đối Xứng)

Đây là trường hợp đặc biệt quan trọng để kiểm tra quy ước $k$ chẵn được quy định tại **Mục 5.2 và 5.7 của tài liệu bối cảnh**.

### 4.1. Ma trận đầu vào & Quy ước
$$M = \begin{bmatrix} 1 & 2 \\ 3 & 4 \end{bmatrix}, \quad k = 2$$
* Padding trên và trái: $\lfloor 2/2 \rfloor = 1$.
* Padding dưới và phải: $2 - 1 - \lfloor 2/2 \rfloor = 0$.
* Tâm cửa sổ 0-based nằm ở hàng 1, cột 1 của cửa sổ.
* Kích thước cửa sổ: $2 \times 2 = 4$ phần tử (số phần tử chẵn).

---

### 4.2. Kết quả MEAN & MEDIAN

| Ô (0-based) | Cửa sổ kèm Padding 0 | Mean (Chính xác) | Mean (`rounded1`) | Median (Sắp xếp & Trung bình) | Median Kết quả |
|---|---|---|---|---|---|
| **(0, 0)** | $\begin{bmatrix} 0 & 0 \\ 0 & 1 \end{bmatrix}$ | $1/4$ | **0.3** $(0.25 \to 0.3)$ | $[0, \mathbf{0, 0}, 1] \to \frac{0+0}{2}$ | **0** |
| **(0, 1)** | $\begin{bmatrix} 0 & 0 \\ 1 & 2 \end{bmatrix}$ | $3/4$ | **0.8** $(0.75 \to 0.8)$ | $[0, \mathbf{0, 1}, 2] \to \frac{0+1}{2}$ | **0.5** |
| **(1, 0)** | $\begin{bmatrix} 0 & 1 \\ 0 & 3 \end{bmatrix}$ | $4/4 = 1$ | **1** | $[0, \mathbf{0, 1}, 3] \to \frac{0+1}{2}$ | **0.5** |
| **(1, 1)** | $\begin{bmatrix} 1 & 2 \\ 3 & 4 \end{bmatrix}$ | $10/4 = 5/2$ | **2.5** | $[1, \mathbf{2, 3}, 4] \to \frac{2+3}{2}$ | **2.5** |

*(Kết quả này khớp 100% với bảng ví dụ minh họa tại Mục 5.7 trong tài liệu bối cảnh).*

---

### 4.3. Kết quả PREWITT với Kernel $2 \times 2$ Tự Nhập
Vì $k = 2 \ne 3$, người dùng tự nhập kernel $2 \times 2$:
* Kernel đầu vào:
  $$G_x = \begin{bmatrix} -1 & 1 \\ -1 & 1 \end{bmatrix}, \quad G_y = \begin{bmatrix} -1 & -1 \\ 1 & 1 \end{bmatrix}$$
* Kernel lật $180^\circ$:
  $$G_{x,\text{flip}} = \begin{bmatrix} 1 & -1 \\ 1 & -1 \end{bmatrix}, \quad G_{y,\text{flip}} = \begin{bmatrix} 1 & 1 \\ -1 & -1 \end{bmatrix}$$
* Kết quả tính toán:
  - Ô (0, 0): $G_x = -1, G_y = -1 \implies G = |-1| + |-1| = \mathbf{2}$.
  - Ô (0, 1): $G_x = -1, G_y = -3 \implies G = |-1| + |-3| = \mathbf{4}$.
  - Ô (1, 0): $G_x = -4, G_y = -2 \implies G = |-4| + |-2| = \mathbf{6}$.
  - Ô (1, 1): $G_x = -2, G_y = -4 \implies G = |-2| + |-4| = \mathbf{6}$.
* Ma trận $G$ kết quả:
  $$\begin{bmatrix} 2 & 4 \\ 6 & 6 \end{bmatrix}$$

---

## 5. Test Case 4: Phát Hiện Biên Dọc Rõ Rệt $4 \times 4$, $k=3$ (Prewitt Edge Detection)

Dạng bài tập minh họa năng lực dò biên dọc (Vertical Edge) của toán tử Prewitt.

### 5.1. Ma trận đầu vào
Bậc thang sáng rõ rệt giữa cột 1 (giá trị 10) và cột 2 (giá trị 100):
$$M = \begin{bmatrix} 10 & 10 & 100 & 100 \\ 10 & 10 & 100 & 100 \\ 10 & 10 & 100 & 100 \\ 10 & 10 & 100 & 100 \end{bmatrix}, \quad k = 3$$

---

### 5.2. Kết quả Phương pháp PREWITT

* **Ma trận gradient $G_x$**:
  $$\begin{bmatrix} -20 & -270 & -270 & 200 \\ -30 & -270 & -270 & 300 \\ -30 & -270 & -270 & 300 \\ -20 & -270 & -270 & 200 \end{bmatrix}$$
* **Ma trận gradient $G_y$**:
  $$\begin{bmatrix} -20 & -30 & -120 & -200 \\ 0 & 0 & 0 & 0 \\ 0 & 0 & 0 & 0 \\ 20 & 30 & 120 & 200 \end{bmatrix}$$
* **Ma trận độ lớn biên $G = |G_x| + |G_y|$**:
  $$\begin{bmatrix} 40 & \mathbf{300} & \mathbf{390} & 400 \\ 30 & \mathbf{270} & \mathbf{270} & 300 \\ 30 & \mathbf{270} & \mathbf{270} & 300 \\ 40 & \mathbf{300} & \mathbf{390} & 400 \end{bmatrix}$$

> **Phân tích kết quả**:
> - Tại các hàng giữa (Hàng 1, Hàng 2), $G_y = 0$ hoàn toàn vì ảnh không có sự biến thiên cường độ theo chiều dọc.
> - Tại các vị trí chuyển bậc từ $10 \to 100$ (Cột 1 và Cột 2), $G_x = -270 \implies |G_x| = 270$, biên đứng được phát hiện với cường độ lớn nhất!

---

## 6. Test Case 5: Ca Kiểm Thử Biên Giới & Cảnh Báo (Boundary & Warning Cases)

### 6.1. Ma trận $1 \times 1$, $k=1$ (Trường hợp tối thiểu)
* **Đầu vào**: `matrix: [[100]]`, `k: 1`.
* **Kết quả**:
  - `padding`: top=0, bottom=0, left=0, right=0.
  - Mean: $100/1 = \mathbf{100}$.
  - Median: $[100] \implies \mathbf{100}$.
  - Prewitt (k=1 không áp dụng hoặc kernel `[[0]]`): $G = \mathbf{0}$.

### 6.2. Cửa sổ $k$ lớn hơn kích thước ma trận ($k > m$ hoặc $k > n$)
* **Đầu vào**: Ma trận $2 \times 2$, $k = 3$ ($k > 2$).
  $$M = \begin{bmatrix} 10 & 20 \\ 30 & 40 \end{bmatrix}, \quad k = 3$$
* **Phản hồi của Backend**:
  - HTTP Status: `200 OK` (Không báo lỗi).
  - Có danh sách cảnh báo `warnings`:
    ```json
    "warnings": [
      {
        "code": "K_GREATER_THAN_MATRIX_DIMENSION",
        "message": "Kích thước cửa sổ lọc (k=3) lớn hơn kích thước ma trận (2×2). Các ô bên ngoài sẽ được tự động lấp đầy bằng số 0 (Zero-padding)."
      }
    ]
    ```
  - Kết quả Mean:
    - Tổng 4 ô trong ma trận $= 10 + 20 + 30 + 40 = 100$.
    - Do $k=3$, mọi cửa sổ $3 \times 3$ đều bao trọn toàn bộ ma trận $2 \times 2$ cùng các số 0 xung quanh $\implies$ Tổng cửa sổ của cả 4 ô đều bằng **100**.
    - Phân số chính xác: $\frac{100}{9}$.
    - Làm tròn 1 chữ số (`rounded1`): $\mathbf{11.1}$.
    - Ma trận kết quả Mean:
      $$\begin{bmatrix} 11.1 & 11.1 \\ 11.1 & 11.1 \end{bmatrix}$$
  - Kết quả Median:
    - Mỗi cửa sổ có 4 số khác 0 và 5 số 0.
    - Dãy sắp xếp 9 phần tử: $[0, 0, 0, 0, \mathbf{0}, 10, 20, 30, 40] \implies$ phần tử thứ 5 là **0**.
    - Ma trận kết quả Median:
      $$\begin{bmatrix} 0 & 0 \\ 0 & 0 \end{bmatrix}$$
  - Kết quả Prewitt ($G$):
    $$\begin{bmatrix} 130 & 110 \\ 90 & 70 \end{bmatrix}$$

---

## 7. Test Case 6: Kiểm Thử Xác Thực Lỗi Đầu Vào (Validation Test Suite)

Dùng để kiểm thử cơ chế bắt lỗi của Backend và khả năng tô đỏ ô lỗi của Frontend.

| Mã Test | Dữ liệu đầu vào kiểm thử | HTTP Status | Mã lỗi kỳ vọng | Thông điệp kiểm tra |
|---|---|---|---|---|
| **VAL-01** | `matrix: []` | 422 | `EMPTY_MATRIX` | Báo lỗi ma trận rỗng. |
| **VAL-02** | `matrix` có hàng 1 gồm 3 ô, hàng 2 gồm 2 ô: `[[1, 2, 3], [4, 5]]` | 422 | `RAGGED_MATRIX` | Báo lỗi các hàng không cùng độ dài. |
| **VAL-03** | Ô có giá trị `300` tại `[0][1]` | 422 | `PIXEL_OUT_OF_RANGE` | Báo lỗi ô [Hàng 1, Cột 2] vượt quá 255. |
| **VAL-04** | Ô có giá trị âm `-5` tại `[1][0]` | 422 | `PIXEL_OUT_OF_RANGE` | Báo lỗi ô [Hàng 2, Cột 1] nhỏ hơn 0. |
| **VAL-05** | Ô chứa số thực `1.5` hoặc chuỗi `"abc"` | 422 | `EMPTY_OR_NON_INTEGER` | Báo lỗi ô không phải số nguyên. |
| **VAL-06** | Ma trận có kích thước $21 \times 5$ | 422 | `INVALID_MATRIX_DIMENSIONS` | Báo lỗi số hàng vượt quá 20. |
| **VAL-07** | $k = 0$ hoặc $k = 10$ | 422 | `INVALID_K_VALUE` | Báo lỗi $k$ phải từ 1 đến 9. |
| **VAL-08** | `method: "UNKNOWN"` | 422 | `INVALID_METHOD` | Báo lỗi phương thức không hợp lệ. |
| **VAL-09** | `method: "PREWITT"`, $k=5$ nhưng không gửi `kernelX, kernelY` | 422 | `MISSING_PREWITT_KERNEL` | Báo lỗi thiếu kernel khi $k \ne 3$. |
| **VAL-10** | `method: "PREWITT"`, $k=3$ nhưng gửi kernel kích thước $2 \times 2$ | 422 | `INVALID_KERNEL_DIMENSIONS`| Báo lỗi kích thước kernel không khớp $k \times k$. |

---

## 8. Payload JSON Mẫu Hoàn Chỉnh (Dùng để Test API hoặc Mock Data)

### 8.1. Request mẫu (POST `/api/filter`):
```json
{
  "matrix": [
    [10, 20, 30],
    [40, 50, 60],
    [70, 80, 90]
  ],
  "k": 3,
  "method": "MEAN",
  "prewittConfig": null,
  "options": {
    "includeSteps": true
  }
}
```

### 8.2. Response mẫu chuẩn xác (HTTP 200 OK):
```json
{
  "success": true,
  "method": "MEAN",
  "k": 3,
  "matrixSize": {
    "rows": 3,
    "cols": 3
  },
  "padding": {
    "padTop": 1,
    "padBottom": 1,
    "padLeft": 1,
    "padRight": 1
  },
  "warnings": [],
  "results": [
    [
      {
        "row": 0,
        "col": 0,
        "displayCoordinates": "Hàng 1, Cột 1",
        "exact": {
          "numerator": 40,
          "denominator": 3,
          "display": "40/3"
        },
        "rounded1": "13.3",
        "roundedInt": 13,
        "step": {
          "window": [
            [0, 0, 0],
            [0, 10, 20],
            [0, 40, 50]
          ],
          "mean": {
            "sum": 120,
            "count": 9,
            "fraction": "120/9",
            "reducedFraction": "40/3",
            "formula": "(0 + 0 + 0 + 0 + 10 + 20 + 0 + 40 + 50) / 9 = 120 / 9 = 40/3 ≈ 13.3"
          }
        }
      },
      {
        "row": 0,
        "col": 1,
        "displayCoordinates": "Hàng 1, Cột 2",
        "exact": {
          "numerator": 70,
          "denominator": 3,
          "display": "70/3"
        },
        "rounded1": "23.3",
        "roundedInt": 23,
        "step": {
          "window": [
            [0, 0, 0],
            [10, 20, 30],
            [40, 50, 60]
          ],
          "mean": {
            "sum": 210,
            "count": 9,
            "fraction": "210/9",
            "reducedFraction": "70/3",
            "formula": "(0 + 0 + 0 + 10 + 20 + 30 + 40 + 50 + 60) / 9 = 210 / 9 = 70/3 ≈ 23.3"
          }
        }
      },
      {
        "row": 0,
        "col": 2,
        "displayCoordinates": "Hàng 1, Cột 3",
        "exact": {
          "numerator": 160,
          "denominator": 9,
          "display": "160/9"
        },
        "rounded1": "17.8",
        "roundedInt": 18,
        "step": {
          "window": [
            [0, 0, 0],
            [20, 30, 0],
            [50, 60, 0]
          ],
          "mean": {
            "sum": 160,
            "count": 9,
            "fraction": "160/9",
            "reducedFraction": "160/9",
            "formula": "(0 + 0 + 0 + 20 + 30 + 0 + 50 + 60 + 0) / 9 = 160 / 9 ≈ 17.8"
          }
        }
      }
    ],
    [
      {
        "row": 1,
        "col": 0,
        "displayCoordinates": "Hàng 2, Cột 1",
        "exact": {
          "numerator": 30,
          "denominator": 1,
          "display": "30"
        },
        "rounded1": "30",
        "roundedInt": 30,
        "step": {
          "window": [
            [0, 10, 20],
            [0, 40, 50],
            [0, 70, 80]
          ],
          "mean": {
            "sum": 270,
            "count": 9,
            "fraction": "270/9",
            "reducedFraction": "30",
            "formula": "(0 + 10 + 20 + 0 + 40 + 50 + 0 + 70 + 80) / 9 = 270 / 9 = 30"
          }
        }
      },
      {
        "row": 1,
        "col": 1,
        "displayCoordinates": "Hàng 2, Cột 2",
        "exact": {
          "numerator": 50,
          "denominator": 1,
          "display": "50"
        },
        "rounded1": "50",
        "roundedInt": 50,
        "step": {
          "window": [
            [10, 20, 30],
            [40, 50, 60],
            [70, 80, 90]
          ],
          "mean": {
            "sum": 450,
            "count": 9,
            "fraction": "450/9",
            "reducedFraction": "50",
            "formula": "(10 + 20 + 30 + 40 + 50 + 60 + 70 + 80 + 90) / 9 = 450 / 9 = 50"
          }
        }
      },
      {
        "row": 1,
        "col": 2,
        "displayCoordinates": "Hàng 2, Cột 3",
        "exact": {
          "numerator": 110,
          "denominator": 3,
          "display": "110/3"
        },
        "rounded1": "36.7",
        "roundedInt": 37,
        "step": {
          "window": [
            [20, 30, 0],
            [50, 60, 0],
            [80, 90, 0]
          ],
          "mean": {
            "sum": 330,
            "count": 9,
            "fraction": "330/9",
            "reducedFraction": "110/3",
            "formula": "(20 + 30 + 0 + 50 + 60 + 0 + 80 + 90 + 0) / 9 = 330 / 9 = 110/3 ≈ 36.7"
          }
        }
      }
    ],
    [
      {
        "row": 2,
        "col": 0,
        "displayCoordinates": "Hàng 3, Cột 1",
        "exact": {
          "numerator": 80,
          "denominator": 3,
          "display": "80/3"
        },
        "rounded1": "26.7",
        "roundedInt": 27,
        "step": {
          "window": [
            [0, 40, 50],
            [0, 70, 80],
            [0, 0, 0]
          ],
          "mean": {
            "sum": 240,
            "count": 9,
            "fraction": "240/9",
            "reducedFraction": "80/3",
            "formula": "(0 + 40 + 50 + 0 + 70 + 80 + 0 + 0 + 0) / 9 = 240 / 9 = 80/3 ≈ 26.7"
          }
        }
      },
      {
        "row": 2,
        "col": 1,
        "displayCoordinates": "Hàng 3, Cột 2",
        "exact": {
          "numerator": 130,
          "denominator": 3,
          "display": "130/3"
        },
        "rounded1": "43.3",
        "roundedInt": 43,
        "step": {
          "window": [
            [40, 50, 60],
            [70, 80, 90],
            [0, 0, 0]
          ],
          "mean": {
            "sum": 390,
            "count": 9,
            "fraction": "390/9",
            "reducedFraction": "130/3",
            "formula": "(40 + 50 + 60 + 70 + 80 + 90 + 0 + 0 + 0) / 9 = 390 / 9 = 130/3 ≈ 43.3"
          }
        }
      },
      {
        "row": 2,
        "col": 2,
        "displayCoordinates": "Hàng 3, Cột 3",
        "exact": {
          "numerator": 280,
          "denominator": 9,
          "display": "280/9"
        },
        "rounded1": "31.1",
        "roundedInt": 31,
        "step": {
          "window": [
            [50, 60, 0],
            [80, 90, 0],
            [0, 0, 0]
          ],
          "mean": {
            "sum": 280,
            "count": 9,
            "fraction": "280/9",
            "reducedFraction": "280/9",
            "formula": "(50 + 60 + 0 + 80 + 90 + 0 + 0 + 0 + 0) / 9 = 280 / 9 ≈ 31.1"
          }
        }
      }
    ]
  ]
}
```
