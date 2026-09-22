# NamSaoHRM – Phần mềm Quản lý nhân sự và Chấm công nội bộ

Ứng dụng web ASP.NET Core MVC (.NET 8) + SQL Server + Bootstrap 5, gồm 8 màn hình: Đăng nhập, Bảng điều khiển, Danh sách nhân viên, Thêm/Sửa nhân viên, Phòng ban & Tài khoản, Chấm công, Lịch sử chấm công, Bảng lương.

## 1. Cần cài đặt

| Phần mềm | Ghi chú |
|---|---|
| Visual Studio 2022 (Community, miễn phí) | Khi cài, chọn workload **ASP.NET and web development**. Workload này cài sẵn .NET 8 SDK và **SQL Server LocalDB**. |
| SQL Server Management Studio (SSMS) | Để xem CSDL và vẽ Database Diagram. |
| Kết nối Internet (lần đầu) | Visual Studio tải 3 gói NuGet: EF Core SqlServer, BCrypt.Net-Next, ClosedXML. |

## 2. Chạy ứng dụng

1. Mở file `NamSaoHRM.sln` bằng Visual Studio 2022.
2. Đợi Visual Studio tự khôi phục gói NuGet (thanh trạng thái dưới cùng).
3. Nhấn **F5**. Nếu được hỏi tin cậy chứng chỉ HTTPS thì chọn **Yes**.
4. Ở lần chạy đầu, ứng dụng **tự tạo CSDL `NamSaoHRM`** trên LocalDB và nạp dữ liệu mẫu: 5 phòng ban, 22 nhân viên, chấm công từ đầu tháng trước và bảng lương tháng trước.

> Dùng SQL Server Express thay cho LocalDB: sửa `DefaultConnection` trong `NamSaoHRM/appsettings.json` theo dòng ghi chú có sẵn trong file.
>
> Muốn tạo lại dữ liệu mẫu từ đầu: trong SSMS, xóa database `NamSaoHRM` rồi chạy lại ứng dụng.

## 3. Tài khoản đăng nhập mẫu

| Vai trò | Tên đăng nhập | Mật khẩu | Ghi chú |
|---|---|---|---|
| Quản trị viên | `admin` | `Admin@123` | Nguyễn Thị Hạnh – Trưởng phòng Kế toán – Nhân sự |
| Nhân viên | `nam.nd` | `123456` | Nguyễn Đức Nam – Thực tập sinh Phòng CNTT (chưa chấm công hôm nay) |
| Nhân viên khác | ví dụ `tuyen.pv`, `long.dh` | `123456` | Xem đủ danh sách ở tab "Tài khoản & phân quyền" |
| Tài khoản bị khóa | `hieu.nv` | `123456` | Dùng để thử thông báo "Tài khoản đã bị khóa" |

## 4. Hướng dẫn chụp ảnh cho báo cáo

Tất cả các trạng thái (lỗi đăng nhập, lỗi validation, đi muộn…) đều do hệ thống xử lý thật, không có nút giả lập.

| Hình | Cách tạo màn hình |
|---|---|
| 3.1 Đăng nhập | Mở trang đầu. Ảnh báo lỗi: nhập `admin` với mật khẩu sai → **"Sai tên đăng nhập hoặc mật khẩu."** |
| 3.2 Bảng điều khiển | Đăng nhập `admin` → trang Bảng điều khiển (thẻ thống kê và biểu đồ theo phòng ban). |
| 3.3 Danh sách nhân viên | Menu **Nhân viên** (có ô tìm kiếm, lọc phòng ban, phân trang). |
| 3.4 Form thêm nhân viên | **Thêm nhân viên** → để trống Họ tên, nhập Email `nam.nd@namsao.vn`, bấm **Lưu** → hiện lỗi đỏ "Họ tên không được bỏ trống." và "Email đã tồn tại trong hệ thống." |
| 3.5 / 3.6 Phòng ban & Tài khoản | Menu **Phòng ban & Tài khoản** → tab 1 (phòng ban) và tab 2 (tài khoản, vai trò, khóa/mở). |
| 3.7 Chấm công | Đăng xuất, đăng nhập `nam.nd`. **Trước khi bấm**: nút Check-in sáng, Check-out mờ. **Sau khi bấm Check-in**: hiện giờ check-in thật; nếu sau 08:15 thì ghi nhận **Đi muộn**; nút Check-out sáng lên. |
| 3.8 Lịch sử chấm công | Menu **Lịch sử chấm công**, chọn tháng trước (dòng đi muộn màu đỏ, về sớm màu cam). |
| 3.9 Bảng lương | Đăng nhập `admin` → **Bảng lương** (mặc định tháng trước). Bấm **Tổng hợp lương** để tính lại. |
| 3.10 File Excel | Nút **Xuất Excel** ở màn hình Bảng lương hoặc Nhân viên → mở file .xlsx và chụp. |
| 3.11 Giao diện điện thoại | Trong Chrome nhấn F12 → biểu tượng điện thoại (Toggle device toolbar) → chọn iPhone. |
| 2.9 Database Diagram | SSMS → kết nối `(localdb)\MSSQLLocalDB` → Databases → NamSaoHRM → chuột phải **Database Diagrams** → New Database Diagram → chọn cả 5 bảng.* |
| 2.11 Solution Explorer | Trong Visual Studio mở rộng các thư mục Controllers, Models, Views, wwwroot. |

\* Nếu SSMS báo *"database does not have a valid owner"*, chạy lệnh sau rồi thử lại (thay bằng tài khoản Windows của bạn):
```sql
ALTER AUTHORIZATION ON DATABASE::NamSaoHRM TO [TEN-MAY\TenDangNhapWindows];
```

## 5. Quy tắc nghiệp vụ (`Services/QuyTacChamCong.cs`)

- Giờ làm 08:00–17:00, nghỉ trưa 12:00–13:00.
- Check-in sau 08:15 là **Đi muộn**; check-out trước 17:00 là **Về sớm**.
- Ngày công: có đủ check-in và check-out = 1 công; chỉ có check-in = 0,5 công.
- Lương = Lương cơ bản / 26 × Ngày công + Phụ cấp (730.000 ₫) − 50.000 ₫ × số lần đi muộn.

## 6. Cấu trúc thư mục

```
NamSaoHRM.sln
Database/Scripts/01_CreateTables.sql   Script tạo 5 bảng (tham khảo, ứng dụng tự tạo CSDL)
NamSaoHRM/
├── Controllers/   Account, Dashboard, NhanVien, PhongBan, TaiKhoan, ChamCong, BangLuong
├── Models/        User, PhongBan, NhanVien, ChamCong, BangLuong, NamSaoDbContext
├── ViewModels/    Dữ liệu cho form và màn hình
├── Services/      Quy tắc chấm công, tính lương, xuất Excel
├── Data/          SeedData – dữ liệu mẫu
├── Views/         Giao diện Razor (Bootstrap 5)
├── wwwroot/       css, js, lib (Bootstrap, Bootstrap Icons, Chart.js)
├── Program.cs
└── appsettings.json
```

> Ở môi trường Development, mỗi lần khởi động ứng dụng sẽ tự bổ sung dữ liệu chấm công **mẫu** cho ngày hôm nay (trừ tài khoản `nam.nd`) để Bảng điều khiển có số liệu khi demo. Xem `Data/SeedData.cs`, hàm `BoSungChamCongHomNay`.
