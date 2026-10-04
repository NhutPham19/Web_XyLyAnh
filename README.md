# WEB_XuLyAnh — Ứng Dụng Web Lọc Ma Trận Ảnh

Ứng dụng web trực quan hóa và tính toán các thuật toán lọc ảnh dạng ma trận điểm ảnh (**Mean / Lọc Trung Bình**, **Median / Lọc Trung Vị**, **Prewitt / Tích chập Gradient lật 180°**).

Dự án phục vụ sinh viên môn **Xử lý ảnh** kiểm tra, đối chiếu bài tập với độ chính xác số học tuyệt đối, trực quan hóa cửa sổ trượt $k \times k$ và xem chi tiết từng bước tính toán.

---

## 🌟 Tính Năng Nổi Bật

- **Tính toán số học chuẩn xác 100%**: Backend tính toán tích chập theo chuẩn tài liệu môn học (Prewitt kernel lật 180°).
- **Trực quan hóa cửa sổ trượt $k \times k$**: Chạm hoặc click vào bất kỳ ô nào (kể cả ô dữ liệu lẫn viền padding 0) để khóa và tô sáng cửa sổ lân cận ngay từ lần ấn đầu tiên, mượt mà trên cả Mobile và Desktop.
- **Padding 0 tự động & Tùy biến**: Tự động sinh $1, 2, \dots$ vòng padding 0 quanh ma trận theo kích thước cửa sổ ($3\times3, 5\times5, \dots$), kèm nút bật/tắt nhanh padding.
- **Nhập liệu nhanh thông minh**:
  - Tùy chỉnh kích thước ma trận $M \times N$ linh hoạt.
  - Công cụ điền nhanh: Điền toàn bộ số, phím tắt điền số 0, 50, 100, 128, 255.
  - Phím điều hướng $\uparrow \downarrow \leftarrow \rightarrow$ và Enter di chuyển giữa các ô như bảng tính Excel.
- **Hiển thị kết quả đa định dạng**: Mặc định hiển thị số nguyên, hỗ trợ số thập phân (1 chữ số) và phân số tối giản.
- **Heatmap màu sắc trực quan**: Tự động phân chia dải màu cho các ô có cùng giá trị số để dễ dàng nhận diện vùng ảnh.
- **Xem chi tiết phép tính**: Nhấp vào từng ô kết quả để xem công thức, danh sách phần tử cửa sổ, trung vị đã sắp xếp, hoặc tích chập $G_x, G_y, G$.

---

## 🛠️ Công Nghệ Sử Dụng

* **Backend**: ASP.NET Core 8 Minimal API (C#) — Xử lý toàn bộ thuật toán tính toán ma trận, lọc biên, tích chập, validation dữ liệu.
* **Frontend**: HTML5, CSS3 hiện đại, Vanilla JavaScript (ES6+) — Nhẹ, tốc độ tải tức thì, responsive mượt mà trên smartphone, không phụ thuộc framework cồng kềnh hay Node.js.
* **Kiến trúc Single-Container**: Backend tích hợp trực tiếp Frontend qua `wwwroot` (`app.UseStaticFiles()`, `app.MapFallbackToFile()`), tạo thành **1 khối duy nhất (Single Deployable Unit)**.
* **Kiểm thử tự động**: 57 test cases xUnit kiểm thử toàn diện các thuật toán và biên ngoại lệ.
* **Container hóa**: Docker & Docker Compose đa tầng (multi-stage build) sẵn sàng triển khai trên mọi nền tảng.

---

## 📁 Cấu Trúc Thư Mục

```text
Web_XuLyAnh/
├── Dockerfile                      # Đóng gói Docker đa tầng (.NET 8 SDK -> Runtime)
├── docker-compose.yml              # Khởi chạy ứng dụng với 1 câu lệnh duy nhất
├── .dockerignore                   # Loại bỏ thư mục rác khi build docker
├── WebXuLyAnh.sln                  # Visual Studio Solution
├── README.md                       # Tài liệu hướng dẫn dự án
│
├── TaiLieuCode_XLA/                # Thư mục đặc tả & kiểm thử chuẩn môn học
│   ├── WEB_XuLyAnh_boi_canh_du_an.md
│   ├── WEB_XuLyAnh_hop_dong_api.md
│   └── WEB_XuLyAnh_du_lieu_test_chuan.md
│
├── backend/                        # Backend ASP.NET Core 8 Minimal API
│   ├── Program.cs                  # Định tuyến API & cấu hình Static Files
│   ├── Models/                     # DTOs (Request / Response)
│   ├── Services/                   # Thuật toán lọc (FilterService)
│   ├── Validators/                 # Kiểm tra tính hợp lệ của ma trận
│   └── wwwroot/                    # Thư mục chứa Frontend tĩnh để chạy tích hợp
│
├── frontend/                       # Mã nguồn giao diện Web (HTML/CSS/JS)
│   ├── index.html                  # Giao diện responsive (Mobile/Desktop)
│   ├── style.css                   # Giao diện tím hiện đại, Heatmap, Fluid Grid
│   └── script.js                   # Logic bắt sự kiện, cửa sổ trượt, gọi API
│
└── tests/                          # 57 Unit & Integration Tests (xUnit)
    └── WebXuLyAnh.Tests/
```

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy

### Cách 1: Dành cho Lập trình viên (Chạy với .NET 8 SDK)

> **Phù hợp khi**: Cần code tính năng mới, sửa thuật toán, gỡ lỗi (debug) hoặc chạy Unit Test.

1. **Yêu cầu môi trường**: Đã cài [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. **Chạy kiểm thử tự động (57 tests)**:
   ```bash
   dotnet test
   ```
3. **Chạy ứng dụng**:
   ```bash
   dotnet run --project backend/WebXuLyAnh.Api.csproj
   ```
4. **Truy cập**: Mở trình duyệt tại [http://localhost:5000](http://localhost:5000).

---

### Cách 2: Khởi chạy siêu tốc bằng Docker (Khuyên dùng cho Demo & Thuyết trình)

> **Phù hợp khi**: Thành viên không cài .NET SDK, người dùng máy Mac/Linux, hoặc khi đi thuyết trình/nộp bài.

1. **Yêu cầu môi trường**: Đã cài [Docker Desktop](https://www.docker.com/products/docker-desktop/).
2. **Khởi chạy bằng Docker Compose**:
   ```bash
   docker compose up -d --build
   ```
3. **Truy cập ngay**: Mở trình duyệt tại [http://localhost:5000](http://localhost:5000).
4. **Dừng container khi không dùng**:
   ```bash
   docker compose down
   ```

---

## 👥 Hướng Dẫn Phối Hợp Nhóm: Tải Code hay Chạy Docker?

| Vai trò thành viên | Phương thức khuyến nghị | Lý do |
| :--- | :--- | :--- |
| **Lập trình viên (Developer)** | **Clone Git & chạy `dotnet run`** | Chỉnh sửa code C# / JS trực tiếp, hot-reload, debug breakpoint, chạy `dotnet test`. Không mất thời gian rebuild image Docker mỗi lần sửa 1 dòng code. |
| **Tester / Thuyết trình / Giảng viên** | **Chạy Docker (`docker compose up`)** | Không cần cài .NET SDK, không sợ sai phiên bản hay xung đột môi trường. Chỉ 1 lệnh là chạy được ngay giao diện hoàn chỉnh. |

---

## 🌐 Chiến Lược Triển Khai (Deployment)

### Có bao nhiêu phần cần Deploy?
Dự án được thiết kế theo kiến trúc **Single Monolithic Container** — Backend ASP.NET Core phục vụ đồng thời cả REST API (`/api/filter`) và Frontend tĩnh (`/wwwroot`).
👉 **Chỉ cần Deploy đúng 1 PHẦN DUY NHẤT!**

### Tại sao nên Deploy 1 phần duy nhất thay vì tách riêng?
1. **Tránh hoàn toàn lỗi CORS**: Frontend và Backend chạy chung origin, không bao giờ bị chặn truy vấn chéo tên miền.
2. **Tiết kiệm tài nguyên**: Chỉ cần 1 server/container duy nhất (tiết kiệm chi phí trên Render, Railway, VPS).
3. **Dễ nộp bài & Đánh giá**: Giảng viên hoặc người xem chỉ cần truy cập 1 đường link duy nhất.
4. **Đồng bộ phiên bản**: Không lo tình trạng Frontend gọi API của phiên bản Backend cũ.

### Các nền tảng Deploy miễn phí & dễ dàng:
- **Render.com / Railway.app**: Kết nối repository GitHub $\rightarrow$ Chọn Deploy Dockerfile $\rightarrow$ Tự động build và cấp link HTTPS miễn phí.
- **VPS (Ubuntu / Debian)**: Cài Docker và chạy `docker compose up -d`.
- **Azure App Service / AWS Lightsail**: Triển khai trực tiếp từ Docker image.
