# WEB_XuLyAnh

Ứng dụng web lọc ảnh dạng ma trận điểm ảnh (Mean, Median, Prewitt).

Dự án phục vụ sinh viên môn **Xử lý ảnh** kiểm tra, đối chiếu bài tập với độ chính xác số học tuyệt đối và xem chi tiết từng bước tính toán.

---

## 📚 Tài Liệu Dự Án

Toàn bộ tài liệu quy chuẩn kỹ thuật và kiểm thử được lưu trữ tại thư mục [`TaiLieuCode_XLA/`](./TaiLieuCode_XLA):

1. **[Bối cảnh & Quy ước Dự án](./TaiLieuCode_XLA/WEB_XuLyAnh_boi_canh_du_an.md)**: Mục tiêu, phạm vi chức năng, quy ước chỉ số, quy ước toán học (Padding 0, Mean, Median, Prewitt lật 180°).
2. **[Hợp đồng API & Schema Chung](./TaiLieuCode_XLA/WEB_XuLyAnh_hop_dong_api.md)**: Đặc tả endpoints (`/api/filter`, `/api/filter/batch`), Request/Response DTOs, cơ chế xem từng bước, quy chuẩn lỗi (validation) và cây thư mục dự án.
3. **[Bộ Dữ Liệu Kiểm Thử Chuẩn](./TaiLieuCode_XLA/WEB_XuLyAnh_du_lieu_test_chuan.md)**: 6 bộ test case mẫu kèm đáp án chi tiết từng ô và mock JSON phục vụ viết unit test và dựng giao diện.

---

## 🛠️ Công Nghệ Sử Dụng

* **Backend**: ASP.NET Core Minimal API (C#) — Đảm nhiệm toàn bộ thuật toán tính toán, làm tròn và padding.
* **Frontend**: React + TypeScript + Vite — Hiển thị ma trận, chuyển đổi chế độ xem (làm tròn 1 số, làm tròn nguyên, phân số) và xem từng bước.
* **Kiến trúc**: Stateless API, triển khai file tĩnh Frontend qua `wwwroot` của Backend.

---

## 📁 Cấu Trúc Thư Mục

```text
Web_XyLyAnh/
│
├── .gitignore                      # Git ignore chung
├── README.md                       # Tài liệu tổng quan dự án
│
├── TaiLieuCode_XLA/                # Thư mục đặc tả & kiểm thử
│   ├── WEB_XuLyAnh_boi_canh_du_an.md
│   ├── WEB_XuLyAnh_hop_dong_api.md
│   └── WEB_XuLyAnh_du_lieu_test_chuan.md
│
├── backend/                        # Backend ASP.NET Core Minimal API
│
└── frontend/                       # Frontend React + Vite
```
