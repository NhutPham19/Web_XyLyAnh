# WEB_XuLyAnh — Tài liệu bối cảnh dự án

Ứng dụng web lọc ảnh dạng ma trận (Mean, Median, Prewitt).

> **Mục đích tài liệu**: chốt lại toàn bộ bối cảnh, quy ước và quyết định của dự án để hai thành viên (BE, FE) và bất kỳ công cụ/trợ lý nào viết tiếp (ví dụ file "hợp đồng hàm chung") đều làm việc trên cùng một sự hiểu.
>
> **Tài liệu này KHÔNG bao gồm**:
> - File thống nhất hàm chung (schema request/response, chữ ký hàm): sẽ do một tài khoản Claude khác viết, và **phải tuân theo tài liệu này**.
> - Dữ liệu test chuẩn (các bài mẫu của giảng viên): sẽ làm sau.
>
> Nếu file hợp đồng hoặc code mâu thuẫn với tài liệu này, hãy hỏi lại chủ dự án thay vì tự quyết định.

---

## 0. Cách đọc trạng thái các quyết định

| Nhãn | Ý nghĩa |
|---|---|
| **[CHỐT]** | Chủ dự án đã nói rõ. |
| **[ĐỀ XUẤT]** | Được đề xuất trong quá trình thảo luận, chưa bị phản đối nhưng chưa được xác nhận rõ ràng. Có thể đổi. |
| **[CHỜ XÁC MINH]** | Phụ thuộc vào bài mẫu/giáo trình của giảng viên, chưa được phép coi là đúng. |

---

## 1. Tổng quan

- **Tên dự án**: WEB_XuLyAnh.
- **Bản chất**: web nhận một **ma trận điểm ảnh** (không phải ảnh thật), áp dụng bộ lọc không gian bằng cửa sổ k×k, trả về ma trận kết quả và (tùy chọn) các bước tính chi tiết.
- **Ba phương pháp**: Mean (trung bình), Median (trung vị), Prewitt (phát hiện biên).
- **Đối tượng dùng**: sinh viên dùng để **kiểm tra bài tập** theo chương trình học. Vì vậy **đúng theo quy ước của giảng viên quan trọng hơn mọi thứ khác**; nếu app khác đáp án giảng viên ở một ô, sinh viên sẽ tin giảng viên.
- **Không làm**: xử lý ảnh thật (upload ảnh, đọc pixel từ file). Chỉ làm việc trên ma trận số do người dùng nhập.
- **Nhóm**: 2 người, một người làm backend, một người làm frontend. Chủ dự án (người viết tài liệu này) là **người làm backend**.

---

## 2. Phạm vi chức năng

1. Nhập ma trận: số hàng, số cột, giá trị từng ô.
2. Chọn kích thước cửa sổ k.
3. Chọn phương pháp (Mean / Median / Prewitt).
4. Prewitt: nhập kernel Gx, Gy; điền sẵn kernel Prewitt mặc định 3×3.
5. Xem ma trận kết quả.
6. Xem chi tiết từng bước tính (tùy chọn, rất hữu ích cho việc học).
7. Chuyển đổi nhanh qua lại các cách hiển thị kết quả đầu ra (xem mục 7).
8. Kiểm tra dữ liệu đầu vào (xem mục 8).

**Yêu cầu phi chức năng**: chạy nhanh với ma trận nhỏ; giao diện dễ nhập (nên hỗ trợ dán từ Excel: phân cách bằng tab/xuống dòng).

**Luồng xử lý tổng quát**: nhập → kiểm tra → padding → duyệt từng điểm → tính → xuất kết quả.

---

## 3. Kiến trúc

| Hạng mục | Quyết định | Trạng thái |
|---|---|---|
| Cơ sở dữ liệu | **Không dùng.** | [CHỐT] |
| Nơi tính toán | **Toàn bộ ở backend**, là nguồn đúng duy nhất. FE **không tự cài thuật toán**, không "xem trước" bằng cách tự tính. | [ĐỀ XUẤT] |
| Kiểu backend | API **không lưu trạng thái** (stateless), một endpoint chính nhận ma trận + k + phương pháp (+ kernel nếu là Prewitt) và trả kết quả + các bước. | [ĐỀ XUẤT] |
| Công nghệ backend | ASP.NET Core Minimal API (C#). | [ĐỀ XUẤT] |
| Công nghệ frontend | React + Vite (FE quen HTML/JS thuần thì vẫn được, hợp đồng không đổi). | [ĐỀ XUẤT] |
| Triển khai | BE phục vụ luôn file tĩnh của FE (thư mục `wwwroot`) để không bị CORS, chỉ deploy một chỗ. Khi phát triển thì FE chạy riêng với proxy. | [ĐỀ XUẤT] |

**Lý do để FE không tự tính**: hai bản code cùng tính một việc sẽ sớm lệch nhau (làm tròn, k chẵn, dấu Prewitt…). FE chỉ làm việc **hiển thị** và chuyển đổi kiểu hiển thị dựa trên dữ liệu BE đã trả.

**Phối hợp**: FE làm việc song song bằng dữ liệu giả (mock) đúng theo hợp đồng; không đợi BE.

---

## 4. Quy ước chỉ số

- Trong **API/dữ liệu nội bộ**: chỉ số **0-based**, hàng trước cột sau (`[hàng][cột]`, `i` là hàng, `j` là cột).
- Trên **giao diện hiển thị cho sinh viên**: dùng **1-based** (sinh viên quen kiểu này). [ĐỀ XUẤT]
- Ma trận là mảng hai chiều, mỗi hàng cùng độ dài.

---

## 5. Quy ước toán học

### 5.1 Ma trận kết quả
Kết quả **cùng kích thước** với ma trận nhập (m×n).

### 5.2 Padding bằng 0 [CHỐT cho k lẻ; k chẵn: CHỜ XÁC MINH]
Ô nằm ngoài ma trận coi như bằng **0**. Số lớp 0 phụ thuộc k (k lớn hơn thì nhiều số 0 hơn; ví dụ k=5 có 2 lớp mỗi phía, k=9 có 4 lớp mỗi phía):

- Padding **trên** và **trái**: `floor(k/2)`.
- Padding **dưới** và **phải**: `k − 1 − floor(k/2)`.

Hệ quả:
- k lẻ: `(k−1)/2` mỗi phía (đối xứng).
- k chẵn: trên/trái = `k/2`, dưới/phải = `k/2 − 1`. Tức là tâm cửa sổ nằm ở vị trí **chỉ số 0-based = k/2**.

Cửa sổ của ô kết quả `(i, j)` (0-based): hàng từ `i − floor(k/2)` đến `i + (k − 1 − floor(k/2))`, cột tương tự.

> **[CHỜ XÁC MINH]** Quy ước k chẵn (và việc "tâm" tính theo chỉ số 0 hay 1) phải đối chiếu với bài mẫu của giảng viên. Cách chọn khác cho kết quả khác hoàn toàn.

BE không cần tạo ma trận padding thật, chỉ coi ô ngoài biên là 0, nhưng phần "xem từng bước" phải **hiện cửa sổ có các số 0** của padding.

### 5.3 Mean (lọc trung bình) [CHỐT]
- Kernel `1/k²` ở mọi vị trí.
- Giá trị = **tổng các phần tử trong cửa sổ (kể cả số 0 của padding) chia cho k²**.

### 5.4 Median (lọc trung vị) [CHỐT]
- Lấy k² giá trị trong cửa sổ **(kể cả số 0 của padding)**, sắp xếp tăng dần.
- Số phần tử **lẻ** (k lẻ): lấy phần tử ở giữa.
- Số phần tử **chẵn** (k chẵn): lấy **trung bình hai giá trị ở giữa**.
- Lưu ý: với k lẻ thì k² luôn lẻ nên trường hợp "chẵn" chỉ xảy ra khi k chẵn.

### 5.5 Prewitt [CHỐT: tích chập, kernel mặc định 3×3]
- Dùng **tích chập** (convolution), không phải tương quan.
- Cách cài đặt thống nhất: **lật kernel 180°** (đảo cả hàng lẫn cột), sau đó nhân từng phần tử với cửa sổ (cửa sổ xác định theo 5.2) rồi cộng lại.
- Tính **Gx** với kernel Gx và **Gy** với kernel Gy.
- Độ lớn: **G = |Gx| + |Gy|**.
- **Giữ nguyên giá trị**: **không** cắt về 0–255, **không** chuẩn hóa.
- Kernel mặc định 3×3 (tên Gx, Gy theo đề cương; "Gx: biên dọc", "Gy: biên ngang"):
  - Gx = `[[-1, 0, 1], [-1, 0, 1], [-1, 0, 1]]`
  - Gy = `[[-1, -1, -1], [0, 0, 0], [1, 1, 1]]`
- Với k ≠ 3: **không có kernel mặc định**, người dùng tự nhập kernel k×k. Kernel phải khớp k×k.
- Với k=3 và kernel mặc định (đối xứng phản xứng), tích chập chỉ làm Gx, Gy đổi dấu so với tương quan; G không đổi, nhưng phần hiển thị Gx, Gy từng bước thì có đổi.

> **[CHỜ XÁC MINH]** Định nghĩa Gx/Gy của giảng viên (có đổi dấu hay đổi chỗ không), và cách tích chập khi k chẵn, phải đối chiếu bài mẫu. Nếu giảng viên định nghĩa khác tài liệu này thì **theo giảng viên**.

### 5.6 Tính chính xác tuyệt đối [ĐỀ XUẤT]
Vì đầu vào là **số nguyên**:
- Mean luôn là phân số với mẫu k².
- Median chỉ ra dạng `x.0` hoặc `x.5`.
- Prewitt (kernel số nguyên) cho kết quả số nguyên.

Nguyên tắc: **tính bằng số nguyên/phân số (tử số, mẫu số)**, tránh sai số số thực (ví dụ 0.1 + 0.2), chỉ làm tròn khi tạo giá trị hiển thị. Cách biểu diễn cụ thể sẽ do file hợp đồng quyết định.

### 5.7 Ví dụ minh họa (chỉ để làm rõ quy ước, KHÔNG phải test chuẩn)
Ma trận `[[1, 2], [3, 4]]`, k = 2 (padding trên/trái = 1, dưới/phải = 0):

| Ô (0-based) | Cửa sổ (kèm padding 0) | Mean (chính xác) | Median |
|---|---|---|---|
| (0,0) | `[[0,0],[0,1]]` | 1/4 = 0.25 | (0+0)/2 = 0 |
| (0,1) | `[[0,0],[1,2]]` | 3/4 = 0.75 | (0+1)/2 = 0.5 |
| (1,0) | `[[0,1],[0,3]]` | 4/4 = 1 | (0+1)/2 = 0.5 |
| (1,1) | `[[1,2],[3,4]]` | 10/4 = 2.5 | (2+3)/2 = 2.5 |

Hiển thị Mean làm tròn 1 chữ số: `[[0.3, 0.8], [1, 2.5]]` (0.25 làm tròn half-up thành 0.3).

---

## 6. Ràng buộc dữ liệu đầu vào

| Mục | Giá trị | Trạng thái |
|---|---|---|
| Giá trị điểm ảnh | Số nguyên 0–255 | [CHỐT] |
| Kích thước ma trận | 1×1 đến 20×20 (m, n ≤ 20) | [CHỐT] (tối đa 20×20) |
| Kích thước cửa sổ k | k ≤ 9 (tối thiểu 1) | [CHỐT] (k ≤ 9) |
| Phương pháp | Mean, Median hoặc Prewitt | [CHỐT] |
| Kernel Prewitt | Kích thước khớp k×k | [CHỐT] |
| Kernel Prewitt: kiểu số | Chỉ nhận **số nguyên** (ví dụ −9 đến 9) để kết quả luôn chính xác | [ĐỀ XUẤT] |
| k lớn hơn kích thước ma trận | **Cho phép** (padding 0 vẫn tính được), chỉ hiện **cảnh báo**, không báo lỗi. Thay thế quy định cũ trong đề cương (báo lỗi). | [ĐỀ XUẤT] |
| Dấu thập phân | Đầu vào là số nguyên nên không nhận số thập phân. Nếu người dùng gõ `1,5` hoặc `1.5` thì báo lỗi rõ ràng. | [ĐỀ XUẤT] |

Dữ liệu vào ở mức tối đa: 400 ô × tối đa 81 giá trị mỗi cửa sổ khoảng 32 nghìn số cho phần "từng bước", hoàn toàn nhẹ.

---

## 7. Hiển thị kết quả và chuyển đổi nhanh

### 7.1 Quy tắc hiển thị mặc định [CHỐT phần chính; chi tiết CHỜ XÁC NHẬN]
- Kết quả hiển thị là **số nguyên** nếu giá trị nguyên.
- Nếu có phần thập phân thì **làm tròn 1 chữ số thập phân**.
- Cách hiểu đang áp dụng (chủ dự án chưa xác nhận lại): sau khi làm tròn 1 chữ số, nếu phần thập phân là `.0` thì hiện số nguyên. Ví dụ `6.04 → 6`, `6.26 → 6.3`, `6.5 → 6.5`.
- Làm tròn kiểu **half-up** (…5 thì lên), **không** dùng "làm tròn về số chẵn" mặc định của C#/Python. Với số âm (Gx, Gy có thể âm) đề xuất làm tròn **half away from zero**. [ĐỀ XUẤT]
- Tránh hiển thị `-0` hoặc `-0.0` (nếu làm tròn ra 0 thì hiện `0`).

### 7.2 Tính năng chuyển đổi nhanh [CHỐT có tính năng; cách hiểu CHỜ XÁC NHẬN]
Chủ dự án yêu cầu có thể **chuyển đổi nhanh qua lại kết quả đầu ra** trên một kết quả đã có. Cách hiểu đang áp dụng (làm cả hai):

1. **Chuyển kiểu hiển thị** trên cùng một kết quả: làm tròn 1 chữ số ⇄ làm tròn nguyên ⇄ giá trị chính xác (phân số).
2. **Giữ kết quả của từng phương pháp** (Mean, Median, Prewitt) thành các tab để xem lại mà không phải tính lại.

Để các nút này **không gọi lại BE** và **không lệch nhau**, BE trả cho mỗi ô cả giá trị chính xác lẫn các bản làm tròn; FE chỉ chọn trường để hiển thị. [ĐỀ XUẤT]

---

## 8. Kiểm tra dữ liệu và lỗi

Các trường hợp cần kiểm tra (BE là nơi quyết định cuối cùng, FE kiểm tra để báo sớm):

- Ô trống hoặc thiếu.
- Ký tự không phải số / không phải số nguyên.
- Giá trị ngoài 0–255.
- Số hàng/cột ngoài 1–20; các hàng không cùng độ dài.
- k ngoài 1–9.
- Kernel Gx/Gy không khớp k×k, hoặc chứa giá trị không hợp lệ.
- Phương pháp không hợp lệ.

Định dạng lỗi (mã lỗi, vị trí ô lỗi, thông điệp tiếng Việt) sẽ được định nghĩa trong file hợp đồng. JSON không có `NaN`/`Infinity`; ô trống phải có quy ước thống nhất (`null` hay `""`).

---

## 9. Phần "xem từng bước"

Nội dung mong muốn cho mỗi ô kết quả (chi tiết cấu trúc dữ liệu thuộc file hợp đồng) [ĐỀ XUẤT]:

- **Mean**: cửa sổ (kèm số 0 padding), tổng, mẫu số k², kết quả.
- **Median**: cửa sổ, danh sách sau khi sắp xếp, vị trí/giá trị lấy, kết quả (nêu rõ trường hợp lấy trung bình hai giá trị giữa khi k chẵn).
- **Prewitt**: cửa sổ, Gx, Gy, G = |Gx| + |Gy|.

Cần giới hạn cách hiển thị trên giao diện (thu gọn/mở rộng, hoặc chỉ xem từng ô được chọn) để không tràn màn hình.

---

## 10. Thiết kế giao diện (từ đề cương)

Ba khu vực: **nhập liệu**, **cấu hình** (k, phương pháp, kernel), **kết quả**. Lưới nhập ma trận phải dùng được trên điện thoại (không để lưới ô cố định quá cứng), và nên hỗ trợ dán từ Excel.

---

## 11. Các điểm còn mở (cần chủ dự án xác nhận)

| # | Nội dung | Mặc định đang áp dụng |
|---|---|---|
| 1 | k chẵn: "tâm" tính theo chỉ số 0 hay 1? (xem 5.2) | Chỉ số 0, tâm tại `k/2` |
| 2 | Định nghĩa Gx/Gy và cách tích chập (xem 5.5) | Như đề cương, lật kernel 180° |
| 3 | k lớn hơn kích thước ma trận | Cho phép + cảnh báo |
| 4 | Cách hiểu quy tắc làm tròn "6.04 → 6, 6.26 → 6.3" (xem 7.1) | Như trên |
| 5 | Cách hiểu tính năng "chuyển đổi nhanh" (xem 7.2) | Làm cả hai kiểu |
| 6 | Kernel Prewitt tự nhập chỉ nhận số nguyên | Chỉ số nguyên (ví dụ −9..9) |
| 7 | Công nghệ BE/FE (mục 3) | ASP.NET Core + React/Vite |

Các điểm 1 và 2 phải được kiểm chứng bằng **bài mẫu của giảng viên** (xem mục 12).

---

## 12. Rủi ro đã biết

- **Lệch quy ước so với giảng viên** (rủi ro lớn nhất): phải kiểm bằng bài mẫu trước khi coi là hoàn thành.
- **Lệch BE/FE** về thứ tự hàng/cột, chỉ số 0/1, ô trống, kiểu số (chuỗi hay số), định dạng lỗi.
- **Sai số/làm tròn khác nhau giữa ngôn ngữ**: Python/C# mặc định làm tròn về số chẵn, JS `toFixed` thì không; dấu `-0`. Giải pháp: chỉ BE làm tròn, tính bằng số nguyên/phân số.
- **Dữ liệu "từng bước" lớn**: giới hạn kích thước và cách hiển thị.
- **An toàn**: phải chặn kích thước ma trận, k ở BE để request lớn không làm treo server.
- **Triển khai**: hosting miễn phí có thể "ngủ", request đầu chậm; cần quy trình Git thống nhất và ai được sửa file hợp đồng.

---

## 13. Việc tiếp theo (ngoài phạm vi tài liệu này)

1. **File thống nhất hàm chung** (schema request/response, định dạng lỗi, chữ ký hàm cho BE, quy ước chỉ số và làm tròn): do tài khoản Claude khác viết, phải tuân theo tài liệu này.
2. **Dữ liệu test chuẩn**: 2–3 bài mẫu có đáp án từ giảng viên (đề, k, phương pháp, kết quả từng ô). Chủ dự án có sẵn, sẽ làm sau. Dữ liệu này sẽ quyết định các điểm [CHỜ XÁC MINH] ở mục 5 và 11.
3. Cả BE và FE (hoặc mock) cùng chạy **chung một bộ test** để tránh lệch.
