# WEB_XuLyAnh — Hợp đồng API & Schema Dữ liệu Chung

> **Tài liệu tham chiếu gốc**: [WEB_XuLyAnh_boi_canh_du_an.md](file:///e:/DoAnCaNhan/Web_XuLyAnh/TaiLieuCode_XLA/WEB_XuLyAnh_boi_canh_du_an.md)  
> **Trạng thái**: [CHỐT] theo bối cảnh dự án, có giá trị bắt buộc áp dụng cho cả Backend (ASP.NET Core) và Frontend (React + Vite).  
> **Phiên bản API**: `v1`

---

## 1. Nguyên tắc cốt lõi & Quy ước chung

1. **Backend là nguồn chân lý duy nhất (Single Source of Truth)**:
   - Toàn bộ thuật toán (padding, tích chập, trung bình, trung vị, lật kernel, tính phân số, làm tròn) được thực thi độc quyền tại Backend.
   - Frontend **tuyệt đối không** tự tính toán hoặc chỉnh sửa giá trị số học. Frontend chỉ làm nhiệm vụ nhập liệu, gửi request và hiển thị kết quả dựa trên các trường Backend đã trả về.
2. **Kiểu giao tiếp & Định dạng**:
   - Giao thức: HTTP RESTful API, định dạng dữ liệu: **JSON** (`Content-Type: application/json; charset=utf-8`).
   - Casing thuộc tính: **camelCase** cho tất cả các trường JSON (ví dụ: `matrixSize`, `stepByStep`, `prewittConfig`).
3. **Quy ước chỉ số (Index)**:
   - **Trong payload API và logic nội bộ**: Chỉ số mảng là **0-based**, hàng trước cột sau (`[row][col]`, `matrix[i][j]`).
   - **Trên giao diện người dùng (UI sinh viên)**: Frontend hiển thị **1-based** (Hàng 1 đến Hàng $m$, Cột 1 đến Cột $n$) để phù hợp với quy ước làm bài tập của sinh viên.
4. **Không dung nạp giá trị vô định**:
   - Tuyệt đối không trả về `NaN`, `Infinity`, `-Infinity` trong JSON.
   - Ô trống đầu vào phải bị từ chối ở bước kiểm tra hợp lệ (validation), không ngầm định thành 0 nếu người dùng bỏ sót ô.
5. **Hỗ trợ chuyển đổi nhanh (Quick Toggle)**:
   - Với mỗi ô kết quả, Backend tính toán và trả về đồng thời:
     - Giá trị phân số chính xác tuyệt đối (`exact`).
     - Giá trị làm tròn 1 chữ số thập phân (`rounded1` - mặc định).
     - Giá trị làm tròn số nguyên (`roundedInt`).
   - Nhờ đó, Frontend cho phép sinh viên click chuyển đổi chế độ xem tức thì **mà không cần gửi thêm request về Backend**.

---

## 2. Danh mục Endpoints

| Phương thức | Đường dẫn | Ý nghĩa | Mô tả |
|---|---|---|---|
| `POST` | `/api/filter` | Xử lý đơn phương thức | Nhận ma trận và 1 phương pháp (`MEAN`, `MEDIAN`, hoặc `PREWITT`), trả về kết quả và các bước tính chi tiết. |
| `POST` | `/api/filter/batch` | Xử lý đa phương thức (Tabs) | Nhận ma trận và danh sách phương pháp, trả về kết quả của cả 3 phương pháp cùng lúc để FE hiển thị các Tab so sánh mà không phải gọi nhiều lần. |
| `GET` | `/api/health` | Health Check | Kiểm tra backend còn hoạt động (phục vụ keep-alive và ping khi deploy trên hosting miễn phí). |

---

## 3. Chi tiết Endpoint: `POST /api/filter`

### 3.1. Request Schema

```json
{
  "matrix": [
    [10, 20, 30],
    [40, 50, 60],
    [70, 80, 90]
  ],
  "k": 3,
  "method": "MEAN",
  "prewittConfig": {
    "kernelX": null,
    "kernelY": null
  },
  "options": {
    "includeSteps": true
  }
}
```

#### Bảng định nghĩa thuộc tính Request:

| Thuộc tính | Kiểu dữ liệu | Bắt buộc | Ràng buộc giá trị | Giải thích |
|---|---|---|---|---|
| `matrix` | `number[][]` (int) | **Có** | - Số hàng $m \in [1, 20]$<br>- Số cột $n \in [1, 20]$<br>- Từng ô: số nguyên $\in [0, 255]$<br>- Các hàng phải cùng độ dài | Ma trận điểm ảnh gốc. |
| `k` | `number` (int) | **Có** | Số nguyên $\in [1, 9]$ | Kích thước cửa sổ lọc $k \times k$. |
| `method` | `string` | **Có** | `"MEAN"` \| `"MEDIAN"` \| `"PREWITT"` | Phương pháp lọc không gian. Không phân biệt hoa/thường khi parse. |
| `prewittConfig` | `object` | Không | Bắt buộc nếu `method == "PREWITT"` và $k \ne 3$ | Cấu hình kernel cho Prewitt. |
| `prewittConfig.kernelX` | `number[][]` (int) | Tùy chọn | Ma trận $k \times k$, số nguyên (ví dụ $-9 \dots 9$) | Kernel $G_x$. Nếu $k=3$ và bỏ trống (`null`), BE tự động dùng kernel mặc định. |
| `prewittConfig.kernelY` | `number[][]` (int) | Tùy chọn | Ma trận $k \times k$, số nguyên (ví dụ $-9 \dots 9$) | Kernel $G_y$. Nếu $k=3$ và bỏ trống (`null`), BE tự động dùng kernel mặc định. |
| `options.includeSteps` | `boolean` | Không | Mặc định: `true` | `true`: trả về chi tiết từng bước cho từng ô; `false`: chỉ trả ma trận kết quả (tiết kiệm băng thông nếu ma trận lớn). |

---

### 3.2. Response Schema (Thành công - `200 OK`)

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
          "numerator": 120,
          "denominator": 9,
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
            "formula": "Tổng các ô = 120, chia k² (9) = 40/3 ≈ 13.3"
          }
        }
      }
    ]
  ]
}
```

#### Chi tiết cấu trúc từng ô kết quả (`CellResultDto`):

| Thuộc tính | Kiểu dữ liệu | Ý nghĩa & Quy tắc sinh dữ liệu |
|---|---|---|
| `row` | `number` | Chỉ số hàng 0-based. |
| `col` | `number` | Chỉ số cột 0-based. |
| `displayCoordinates` | `string` | Tọa độ 1-based hiển thị thân thiện (ví dụ `"Hàng 1, Cột 1"`). |
| `exact.numerator` | `number` | Tử số của giá trị chính xác (rút gọn tối giản). |
| `exact.denominator` | `number` | Mẫu số của giá trị chính xác (mẫu số dương, rút gọn tối giản). |
| `exact.display` | `string` | Chuỗi hiển thị phân số rút gọn: `"40/3"`, nếu mẫu số là 1 thì hiện số nguyên `"50"`. |
| `rounded1` | `string` | **Giá trị mặc định hiển thị trên bảng**: Làm tròn 1 chữ số theo quy tắc Half-up (âm thì away from zero). **Nếu sau làm tròn là `.0` thì chỉ hiển thị số nguyên** (ví dụ: `6.04 -> "6"`, `6.26 -> "6.3"`, `0.25 -> "0.3"`). Tránh hiển thị `"-0"` hoặc `"-0.0"`. |
| `roundedInt` | `number` | Giá trị làm tròn về số nguyên gần nhất theo quy tắc Half-up (ví dụ: `0.3 -> 0`, `0.8 -> 1`, `2.5 -> 3`). |
| `step` | `object` | Chi tiết từng bước tính toán tại ô `(row, col)`. (Chỉ có khi `includeSteps == true`). |

---

### 3.3. Cấu trúc chi tiết bước tính (`step`) theo từng phương pháp

#### A. Khi `method == "MEAN"`:
```json
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
```

#### B. Khi `method == "MEDIAN"`:
```json
"step": {
  "window": [
    [0, 0, 0],
    [0, 10, 20],
    [0, 40, 50]
  ],
  "median": {
    "rawElements": [0, 0, 0, 0, 10, 20, 0, 40, 50],
    "sortedElements": [0, 0, 0, 0, 0, 10, 20, 40, 50],
    "pickedIndices": [4],
    "pickedValues": [0],
    "isEven": false,
    "formula": "Dãy sắp xếp: [0, 0, 0, 0, 0, 10, 20, 40, 50]. Số phần tử lẻ (9), lấy phần tử thứ 5 tại chỉ số 4: giá trị = 0."
  }
}
```
*(Nếu $k$ chẵn, `isEven: true`, `pickedIndices: [1, 2]`, `pickedValues: [1, 2]`, `formula: "Dãy chẵn: lấy trung bình (1 + 2) / 2 = 1.5"`).*

#### C. Khi `method == "PREWITT"`:
```json
"step": {
  "window": [
    [10, 20, 30],
    [40, 50, 60],
    [70, 80, 90]
  ],
  "prewitt": {
    "kernelXOriginal": [
      [-1, 0, 1],
      [-1, 0, 1],
      [-1, 0, 1]
    ],
    "kernelYOriginal": [
      [-1, -1, -1],
      [0, 0, 0],
      [1, 1, 1]
    ],
    "kernelXFlipped": [
      [1, 0, -1],
      [1, 0, -1],
      [1, 0, -1]
    ],
    "kernelYFlipped": [
      [1, 1, 1],
      [0, 0, 0],
      [-1, -1, -1]
    ],
    "gx": -60,
    "gy": -180,
    "absGx": 60,
    "absGy": 180,
    "g": 240,
    "formulaGx": "Tích chập Gx (kernel lật 180°): (10*1 + 40*1 + 70*1) + (30*(-1) + 60*(-1) + 90*(-1)) = 120 - 180 = -60",
    "formulaGy": "Tích chập Gy (kernel lật 180°): (10*1 + 20*1 + 30*1) + (70*(-1) + 80*(-1) + 90*(-1)) = 60 - 240 = -180",
    "formulaG": "|Gx| + |Gy| = |-60| + |-180| = 60 + 180 = 240"
  }
}
```

---

## 4. Chi tiết Endpoint: `POST /api/filter/batch`

Hỗ trợ Frontend hiển thị đồng thời 3 tab kết quả (Mean, Median, Prewitt) với 1 request duy nhất.

### 4.1. Request Schema
```json
{
  "matrix": [
    [10, 20, 30],
    [40, 50, 60],
    [70, 80, 90]
  ],
  "k": 3,
  "methods": ["MEAN", "MEDIAN", "PREWITT"],
  "prewittConfig": {
    "kernelX": null,
    "kernelY": null
  },
  "options": {
    "includeSteps": true
  }
}
```

### 4.2. Response Schema
```json
{
  "success": true,
  "matrixSize": { "rows": 3, "cols": 3 },
  "k": 3,
  "warnings": [],
  "data": {
    "mean": { /* Cấu trúc như kết quả của /api/filter với method MEAN */ },
    "median": { /* Cấu trúc như kết quả của /api/filter với method MEDIAN */ },
    "prewitt": { /* Cấu trúc như kết quả của /api/filter với method PREWITT */ }
  }
}
```

---

## 5. Quy chuẩn Xử lý Lỗi (Validation & Error Schema)

Khi dữ liệu đầu vào không hợp lệ, Backend trả về HTTP Status code tương ứng cùng cấu trúc lỗi chuẩn xác để Frontend có thể **tô đỏ chính xác ô bị lỗi** trên lưới nhập.

### 5.1. HTTP Status Codes
* `200 OK`: Xử lý thành công.
* `400 Bad Request`: Định dạng request không hợp lệ (sai cú pháp JSON, thiếu field gốc).
* `422 Unprocessable Entity`: Dữ liệu vi phạm nghiệp vụ logic (ô trống, số âm, số vượt 255, $k$ ngoài 1..9, kernel sai kích thước...).
* `500 Internal Server Error`: Lỗi hệ thống không lường trước.

### 5.2. Error Response Schema
```json
{
  "success": false,
  "statusCode": 422,
  "errorSummary": "Dữ liệu ma trận đầu vào không hợp lệ. Vui lòng kiểm tra lại các ô bị đánh dấu.",
  "errors": [
    {
      "field": "matrix",
      "row": 1,
      "col": 2,
      "code": "PIXEL_OUT_OF_RANGE",
      "message": "Ô [Hàng 2, Cột 3] có giá trị 300 vượt quá giới hạn cho phép (0 - 255)."
    },
    {
      "field": "matrix",
      "row": 0,
      "col": 1,
      "code": "EMPTY_OR_NON_INTEGER",
      "message": "Ô [Hàng 1, Cột 2] không được để trống hoặc chứa ký tự không phải số nguyên."
    }
  ]
}
```

### 5.3. Bảng danh mục mã lỗi chuẩn (Error Codes)

| Mã lỗi (`code`) | HTTP Status | Điều kiện kích hoạt | Thông điệp mẫu tiếng Việt |
|---|---|---|---|
| `EMPTY_MATRIX` | 422 | `matrix` rỗng hoặc `null` | Ma trận không được để trống. |
| `INVALID_MATRIX_DIMENSIONS` | 422 | Số hàng $m$ hoặc cột $n$ ngoài khoảng $[1, 20]$ | Kích thước ma trận phải từ 1×1 đến tối đa 20×20. |
| `RAGGED_MATRIX` | 422 | Các hàng có số lượng cột không đều nhau | Các hàng trong ma trận phải có cùng số lượng phần tử. |
| `EMPTY_OR_NON_INTEGER` | 422 | Ô bị để trống, chứa chuỗi chữ, hoặc số thập phân (`1.5`, `1,5`) | Ô [Hàng {r}, Cột {c}] phải là số nguyên, không được để trống hoặc là số thập phân. |
| `PIXEL_OUT_OF_RANGE` | 422 | Điểm ảnh $< 0$ hoặc $> 255$ | Giá trị điểm ảnh tại ô [Hàng {r}, Cột {c}] phải nằm trong đoạn [0, 255]. |
| `INVALID_K_VALUE` | 422 | $k < 1$ hoặc $k > 9$ | Kích thước cửa sổ k phải là số nguyên trong khoảng [1, 9]. |
| `INVALID_METHOD` | 422 | `method` không thuộc MEAN, MEDIAN, PREWITT | Phương pháp lọc không hợp lệ. Chỉ chấp nhận MEAN, MEDIAN hoặc PREWITT. |
| `MISSING_PREWITT_KERNEL` | 422 | $k \ne 3$ nhưng không truyền kernel $G_x, G_y$ | Với kích thước k ≠ 3, bạn phải tự cung cấp ma trận kernel Gx và Gy tương ứng kích thước k×k. |
| `INVALID_KERNEL_DIMENSIONS`| 422 | Kích thước kernel không khớp $k \times k$ | Ma trận kernel Prewitt phải có kích thước đúng bằng k×k ({k}×{k}). |
| `INVALID_KERNEL_VALUE` | 422 | Kernel chứa số thực hoặc ký tự lạ | Các giá trị trong kernel Prewitt phải là số nguyên. |

### 5.4. Quy ước Cảnh báo (Warnings)
Khi $k$ lớn hơn kích thước ma trận ($k > m$ hoặc $k > n$):
* **Không coi là lỗi** (Status `200 OK`).
* Backend trả về danh sách `warnings`:
```json
"warnings": [
  {
    "code": "K_GREATER_THAN_MATRIX_DIMENSION",
    "message": "Kích thước cửa sổ lọc (k=3) lớn hơn kích thước ma trận (2×2). Các ô bên ngoài sẽ được tự động lấp đầy bằng số 0 (Zero-padding)."
  }
]
```

---

## 6. Định nghĩa Kiểu Dữ liệu (DTO Code Samples)

### 6.1. Backend C# (ASP.NET Core Minimal API)

```csharp
namespace WebXuLyAnh.Api.Models;

public record MatrixFilterRequest(
    int[][] Matrix,
    int K,
    string Method,
    PrewittConfigDto? PrewittConfig = null,
    FilterOptionsDto? Options = null
);

public record PrewittConfigDto(
    int[][]? KernelX,
    int[][]? KernelY
);

public record FilterOptionsDto(
    bool IncludeSteps = true
);

public record BatchFilterRequest(
    int[][] Matrix,
    int K,
    List<string> Methods,
    PrewittConfigDto? PrewittConfig = null,
    FilterOptionsDto? Options = null
);

public record FractionDto(
    int Numerator,
    int Denominator,
    string Display
);

public record CellResultDto(
    int Row,
    int Col,
    string DisplayCoordinates,
    FractionDto Exact,
    string Rounded1,
    int RoundedInt,
    StepDetailDto? Step
);

public record PaddingInfoDto(
    int PadTop,
    int PadBottom,
    int PadLeft,
    int PadRight
);

public record StepDetailDto(
    int[][] Window,
    MeanStepDto? Mean = null,
    MedianStepDto? Median = null,
    PrewittStepDto? Prewitt = null
);

public record MeanStepDto(
    int Sum,
    int Count,
    string Fraction,
    string ReducedFraction,
    string Formula
);

public record MedianStepDto(
    int[] RawElements,
    int[] SortedElements,
    int[] PickedIndices,
    int[] PickedValues,
    bool IsEven,
    string Formula
);

public record PrewittStepDto(
    int[][] KernelXOriginal,
    int[][] KernelYOriginal,
    int[][] KernelXFlipped,
    int[][] KernelYFlipped,
    int Gx,
    int Gy,
    int AbsGx,
    int AbsGy,
    int G,
    string FormulaGx,
    string FormulaGy,
    string FormulaG
);

public record FilterResponse(
    bool Success,
    string Method,
    int K,
    MatrixSizeDto MatrixSize,
    PaddingInfoDto Padding,
    List<WarningDto> Warnings,
    CellResultDto[][] Results
);

public record MatrixSizeDto(int Rows, int Cols);
public record WarningDto(string Code, string Message);

public record ValidationErrorDetail(
    string Field,
    int? Row,
    int? Col,
    string Code,
    string Message
);

public record ErrorResponse(
    bool Success,
    int StatusCode,
    string ErrorSummary,
    List<ValidationErrorDetail> Errors
);
```

### 6.2. Frontend TypeScript Interfaces (React + Vite)

```typescript
// types/api.ts

export type FilterMethod = 'MEAN' | 'MEDIAN' | 'PREWITT';

export interface PrewittConfig {
  kernelX?: number[][] | null;
  kernelY?: number[][] | null;
}

export interface FilterOptions {
  includeSteps?: boolean;
}

export interface MatrixFilterRequest {
  matrix: number[][];
  k: number;
  method: FilterMethod;
  prewittConfig?: PrewittConfig;
  options?: FilterOptions;
}

export interface BatchFilterRequest {
  matrix: number[][];
  k: number;
  methods: FilterMethod[];
  prewittConfig?: PrewittConfig;
  options?: FilterOptions;
}

export interface Fraction {
  numerator: number;
  denominator: number;
  display: string;
}

export interface MeanStep {
  sum: number;
  count: number;
  fraction: string;
  reducedFraction: string;
  formula: string;
}

export interface MedianStep {
  rawElements: number[];
  sortedElements: number[];
  pickedIndices: number[];
  pickedValues: number[];
  isEven: boolean;
  formula: string;
}

export interface PrewittStep {
  kernelXOriginal: number[][];
  kernelYOriginal: number[][];
  kernelXFlipped: number[][];
  kernelYFlipped: number[][];
  gx: number;
  gy: number;
  absGx: number;
  absGy: number;
  g: number;
  formulaGx: string;
  formulaGy: string;
  formulaG: string;
}

export interface StepDetail {
  window: number[][];
  mean?: MeanStep;
  median?: MedianStep;
  prewitt?: PrewittStep;
}

export interface CellResult {
  row: number;
  col: number;
  displayCoordinates: string;
  exact: Fraction;
  rounded1: string;     // Dùng làm giá trị hiển thị mặc định
  roundedInt: number;   // Dùng khi chọn chế độ "Làm tròn số nguyên"
  step?: StepDetail;
}

export interface Warning {
  code: string;
  message: string;
}

export interface FilterResponse {
  success: boolean;
  method: FilterMethod;
  k: number;
  matrixSize: { rows: number; cols: number };
  padding: { padTop: number; padBottom: number; padLeft: number; padRight: number };
  warnings: Warning[];
  results: CellResult[][];
}

export interface BatchFilterResponse {
  success: boolean;
  matrixSize: { rows: number; cols: number };
  k: number;
  warnings: Warning[];
  data: {
    mean?: FilterResponse;
    median?: FilterResponse;
    prewitt?: FilterResponse;
  };
}

export interface ValidationErrorDetail {
  field: string;
  row?: number;
  col?: number;
  code: string;
  message: string;
}

export interface ApiErrorResponse {
  success: false;
  statusCode: number;
  errorSummary: string;
  errors: ValidationErrorDetail[];
}

// Chế độ hiển thị kết quả trên UI (FE View Toggle Mode)
export type ResultDisplayMode = 'ROUNDED_1' | 'ROUNDED_INT' | 'EXACT_FRACTION';
```

---

## 7. Quy tắc Thuật toán Chi tiết (Chuẩn hóa code cho BE)

### 7.1. Padding ma trận
Với ma trận $m \times n$, cửa sổ $k \times k$:
* `padTop = floor(k / 2)`
* `padLeft = floor(k / 2)`
* `padBottom = k - 1 - floor(k / 2)`
* `padRight = k - 1 - floor(k / 2)`
* Giá trị cửa sổ cho ô $(i, j)$ với $r \in [-\text{padTop}, \text{padBottom}]$, $c \in [-\text{padLeft}, \text{padRight}]$:
  $$\text{window}[r + \text{padTop}][c + \text{padLeft}] = \begin{cases} \text{matrix}[i + r][j + c] & \text{nếu } 0 \le i + r < m \text{ và } 0 \le j + c < n \\ 0 & \text{ngược lại (Zero-padding)} \end{cases}$$

### 7.2. Lọc trung bình (Mean)
* $\text{sum} = \sum_{u=0}^{k-1} \sum_{v=0}^{k-1} \text{window}[u][v]$
* Phân số chính xác: $\frac{\text{sum}}{k^2}$ (rút gọn chia cho $\gcd(\text{sum}, k^2)$).

### 7.3. Lọc trung vị (Median)
* Trải phẳng $k^2$ phần tử của `window` thành mảng 1 chiều, sắp xếp tăng dần: $S = [s_0, s_1, \dots, s_{k^2-1}]$.
* Nếu $k^2$ lẻ ($k$ lẻ): Lấy phần tử tại chỉ số $\lfloor k^2 / 2 \rfloor$.
* Nếu $k^2$ chẵn ($k$ chẵn): Lấy trung bình cộng 2 phần tử giữa $\frac{S[k^2/2 - 1] + S[k^2/2]}{2}$.

### 7.4. Lọc Prewitt
* **Lật kernel 180°**:
  $$\text{kernelFlipped}[u][v] = \text{kernel}[k - 1 - u][k - 1 - v]$$
* Kernel mặc định khi $k = 3$:
  $$G_x = \begin{bmatrix} -1 & 0 & 1 \\ -1 & 0 & 1 \\ -1 & 0 & 1 \end{bmatrix} \xrightarrow{\text{Lật } 180^\circ} G_{x,\text{flip}} = \begin{bmatrix} 1 & 0 & -1 \\ 1 & 0 & -1 \\ 1 & 0 & -1 \end{bmatrix}$$
  $$G_y = \begin{bmatrix} -1 & -1 & -1 \\ 0 & 0 & 0 \\ 1 & 1 & 1 \end{bmatrix} \xrightarrow{\text{Lật } 180^\circ} G_{y,\text{flip}} = \begin{bmatrix} 1 & 1 & 1 \\ 0 & 0 & 0 \\ -1 & -1 & -1 \end{bmatrix}$$
* $G_x = \sum_{u=0}^{k-1} \sum_{v=0}^{k-1} \text{window}[u][v] \times G_{x,\text{flip}}[u][v]$
* $G_y = \sum_{u=0}^{k-1} \sum_{v=0}^{k-1} \text{window}[u][v] \times G_{y,\text{flip}}[u][v]$
* Kết quả độ lớn: $G = |G_x| + |G_y|$ (giữ nguyên số nguyên, không cắt về 255).

### 7.5. Thuật toán làm tròn Half-up & Định dạng chuỗi
```csharp
public static class RoundingHelper
{
    // Làm tròn 1 chữ số thập phân (Half-up, âm thì away from zero)
    public static string FormatRounded1(int numerator, int denominator)
    {
        if (numerator == 0) return "0";
        bool isNegative = (numerator < 0) ^ (denominator < 0);
        long absNum = Math.Abs((long)numerator);
        long absDen = Math.Abs((long)denominator);

        // x * 10 + 0.5 = (absNum * 10 * 2 + absDen) / (absDen * 2)
        long scaledPlusHalf = (absNum * 20 + absDen) / (absDen * 2);
        
        long integerPart = scaledPlusHalf / 10;
        long decimalPart = scaledPlusHalf % 10;

        string prefix = isNegative && (integerPart > 0 || decimalPart > 0) ? "-" : "";

        if (decimalPart == 0)
        {
            return $"{prefix}{integerPart}";
        }
        return $"{prefix}{integerPart}.{decimalPart}";
    }

    // Làm tròn số nguyên (Half-up, âm thì away from zero)
    public static int FormatRoundedInt(int numerator, int denominator)
    {
        if (numerator == 0) return 0;
        bool isNegative = (numerator < 0) ^ (denominator < 0);
        long absNum = Math.Abs((long)numerator);
        long absDen = Math.Abs((long)denominator);

        long roundedAbs = (absNum * 2 + absDen) / (absDen * 2);
        return isNegative ? -(int)roundedAbs : (int)roundedAbs;
    }
}
```


---

## 8. Quy chuẩn Cấu trúc Cây Thư mục Dự án (Monorepo)

Để phân định trách nhiệm rõ ràng giữa 2 thành viên (Backend và Frontend), tránh xung đột mã nguồn trên Git:

```text
Web_XyLyAnh/
│
├── .gitignore                      # Ignore chung cho cả .NET và Node
├── README.md                       # Hướng dẫn chạy dự án, giới thiệu đề tài
│
├── TaiLieuCode_XLA/                # Toàn bộ tài liệu đặc tả & kiểm thử
│   ├── WEB_XuLyAnh_boi_canh_du_an.md
│   ├── WEB_XuLyAnh_hop_dong_api.md
│   └── WEB_XuLyAnh_du_lieu_test_chuan.md
│
├── backend/                        # Mã nguồn Backend (ASP.NET Core Minimal API)
│   ├── WebXuLyAnh.Api.csproj
│   ├── Program.cs                  # Khởi tạo Minimal API & Endpoints
│   ├── Models/                     # DTOs Request/Response
│   ├── Services/                   # Lõi thuật toán lọc ảnh (Mean, Median, Prewitt)
│   ├── Helpers/                    # Hỗ trợ toán phân số & làm tròn Half-up
│   ├── Validators/                 # Xác thực dữ liệu đầu vào
│   ├── wwwroot/                    # Thư mục chứa file tĩnh FE khi deploy
│   └── appsettings.json
│
├── frontend/                       # Mã nguồn Frontend (React + Vite)
│   ├── package.json
│   ├── vite.config.ts              # Proxy /api sang Backend khi dev
│   ├── tsconfig.json
│   ├── index.html
│   └── src/
│       ├── types/                  # TypeScript Interfaces
│       ├── services/               # Hàm gọi API Backend
│       ├── components/             # Components UI (Lưới nhập, Bảng kết quả, Xem bước)
│       ├── App.tsx
│       └── main.tsx
│
└── tests/                          # Dự án kiểm thử tự động Backend (xUnit)
    └── WebXuLyAnh.Tests/
```
