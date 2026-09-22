using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;
using NamSaoHRM.Services;

namespace NamSaoHRM.Data;

/// <summary>
/// Khởi tạo CSDL và dữ liệu mẫu (phòng ban, nhân viên, tài khoản, chấm công, bảng lương).
/// Chỉ chạy khi CSDL còn trống. Mật khẩu mặc định: admin / Admin@123, các tài khoản khác / 123456.
/// </summary>
public static class SeedData
{
    public const string DemoNhanVien = "nam.nd";

    public static void Initialize(NamSaoDbContext context, bool laMoiTruongDev)
    {
        context.Database.EnsureCreated();

        if (!context.PhongBans.Any())
        {
            TaoPhongBanVaNhanVien(context);
            TaoChamCongLichSu(context);
            var thangTruoc = DateTime.Today.AddMonths(-1);
            new BangLuongService(context).TongHopAsync(thangTruoc.Month, thangTruoc.Year).GetAwaiter().GetResult();
        }

        // Môi trường Development: bổ sung dữ liệu chấm công mẫu cho ngày hôm nay
        // để trang Tổng quan có số liệu khi chạy demo (không áp dụng cho tài khoản demo nhân viên).
        if (laMoiTruongDev) BoSungChamCongHomNay(context);
    }

    private static readonly (string Ten, string MoTa, DateTime NgayTL)[] DsPhongBan =
    {
        ("Ban Giám đốc", "Điều hành chung hoạt động của công ty", new DateTime(2018, 3, 1)),
        ("Phòng Kinh doanh", "Phát triển khách hàng, bán hàng trực tuyến", new DateTime(2018, 3, 1)),
        ("Phòng Marketing", "Quảng bá thương hiệu, quản lý kênh truyền thông", new DateTime(2019, 6, 15)),
        ("Phòng Kế toán - Nhân sự", "Kế toán tài chính, tuyển dụng, chấm công và tiền lương", new DateTime(2018, 3, 1)),
        ("Phòng Công nghệ thông tin", "Vận hành hệ thống, website và phần mềm nội bộ", new DateTime(2020, 1, 10)),
    };

    // (Họ tên, Giới tính, Phòng ban index, Chức vụ, Lương cơ bản (triệu), Username đặc biệt)
    private static readonly (string HoTen, string GT, int PB, string ChucVu, int Luong, string? User)[] DsNhanVien =
    {
        ("Trần Minh Quang", "Nam", 0, "Giám đốc", 35, null),
        ("Lê Thị Thu Hà", "Nữ", 0, "Phó Giám đốc", 28, null),
        ("Đỗ Hoàng Long", "Nam", 1, "Trưởng phòng", 20, null),
        ("Nguyễn Thị Mai Anh", "Nữ", 1, "Nhân viên kinh doanh", 11, null),
        ("Vũ Quốc Bảo", "Nam", 1, "Nhân viên kinh doanh", 11, null),
        ("Phan Thanh Hương", "Nữ", 1, "Nhân viên kinh doanh", 10, null),
        ("Bùi Đức Thắng", "Nam", 1, "Nhân viên kinh doanh", 10, null),
        ("Hoàng Thị Lan", "Nữ", 1, "Chăm sóc khách hàng", 9, null),
        ("Ngô Văn Hiếu", "Nam", 1, "Chăm sóc khách hàng", 9, null),
        ("Đặng Thùy Linh", "Nữ", 2, "Trưởng phòng", 19, null),
        ("Trịnh Minh Khoa", "Nam", 2, "Chuyên viên Digital Marketing", 13, null),
        ("Lý Thị Ngọc Ánh", "Nữ", 2, "Chuyên viên nội dung", 11, null),
        ("Mai Xuân Trường", "Nam", 2, "Thiết kế đồ họa", 12, null),
        ("Nguyễn Thị Hạnh", "Nữ", 3, "Trưởng phòng", 20, "admin"),
        ("Cao Thị Phương", "Nữ", 3, "Kế toán tổng hợp", 13, null),
        ("Lương Văn Toàn", "Nam", 3, "Kế toán viên", 11, null),
        ("Tạ Thị Hồng Nhung", "Nữ", 3, "Chuyên viên nhân sự", 11, null),
        ("Phạm Văn Tuyên", "Nam", 4, "Trưởng phòng", 25, null),
        ("Hồ Anh Tuấn", "Nam", 4, "Lập trình viên", 18, null),
        ("Kiều Thị Thanh Tâm", "Nữ", 4, "Kiểm thử phần mềm", 14, null),
        ("Dương Minh Đức", "Nam", 4, "Quản trị hệ thống", 16, null),
        ("Nguyễn Đức Nam", "Nam", 4, "Thực tập sinh", 5, DemoNhanVien),
    };

    private static void TaoPhongBanVaNhanVien(NamSaoDbContext context)
    {
        var phongBans = DsPhongBan.Select(p => new PhongBan { TenPhongBan = p.Ten, MoTa = p.MoTa, NgayThanhLap = p.NgayTL }).ToList();
        context.PhongBans.AddRange(phongBans);
        context.SaveChanges();

        var rnd = new Random(2026);
        var daDung = new HashSet<string>();
        foreach (var (hoTen, gt, pb, chucVu, luong, userDacBiet) in DsNhanVien)
        {
            var username = userDacBiet ?? TaoUsername(hoTen, daDung);
            daDung.Add(username);
            var laAdmin = username == "admin";

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(laAdmin ? "Admin@123" : "123456", 10),
                Role = laAdmin ? VaiTro.Admin : VaiTro.NhanVien,
                TrangThai = true,
                NgayTao = DateTime.Now.AddMonths(-3)
            };
            context.Users.Add(user);
            context.SaveChanges();

            var emailUser = laAdmin ? "hanh.nt" : username;
            context.NhanViens.Add(new NhanVien
            {
                UserID = user.UserID,
                MaPhongBan = phongBans[pb].MaPhongBan,
                HoTen = hoTen,
                GioiTinh = gt,
                NgaySinh = new DateTime(1975 + rnd.Next(0, 28), rnd.Next(1, 13), rnd.Next(1, 29)),
                Email = $"{emailUser}@namsao.vn",
                SoDienThoai = "09" + rnd.Next(10000000, 99999999),
                ChucVu = chucVu,
                LuongCoBan = luong * 1_000_000m,
                NgayVaoLam = userDacBiet == DemoNhanVien ? new DateTime(2026, 6, 1)
                    : new DateTime(2018 + rnd.Next(0, 7), rnd.Next(1, 13), rnd.Next(1, 29))
            });
            context.SaveChanges();
        }

        // Một tài khoản bị khóa để minh họa chức năng khóa tài khoản
        var nvKhoa = context.NhanViens.First(n => n.HoTen == "Ngô Văn Hiếu");
        var tkKhoa = context.Users.First(u => u.UserID == nvKhoa.UserID);
        tkKhoa.TrangThai = false;
        context.SaveChanges();
    }

    private static void TaoChamCongLichSu(NamSaoDbContext context)
    {
        var rnd = new Random(20260601);
        var tuNgay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
        var nhanViens = context.NhanViens.ToList();

        for (var ngay = tuNgay; ngay < DateTime.Today; ngay = ngay.AddDays(1))
        {
            if (!QuyTacChamCong.LaNgayLamViec(ngay)) continue;
            foreach (var nv in nhanViens)
            {
                if (ngay < nv.NgayVaoLam) continue;
                if (rnd.NextDouble() < 0.03) continue; // vắng mặt
                var vao = ngay + (rnd.NextDouble() < 0.86
                    ? TimeSpan.FromMinutes(rnd.Next(7 * 60 + 35, 8 * 60 + 15))
                    : TimeSpan.FromMinutes(rnd.Next(8 * 60 + 16, 9 * 60 + 5)));
                var ra = ngay + (rnd.NextDouble() < 0.88
                    ? TimeSpan.FromMinutes(rnd.Next(17 * 60, 18 * 60 + 15))
                    : TimeSpan.FromMinutes(rnd.Next(16 * 60 + 5, 16 * 60 + 58)));
                context.ChamCongs.Add(new ChamCong
                {
                    MaNV = nv.MaNV,
                    NgayChamCong = ngay,
                    GioCheckIn = vao,
                    GioCheckOut = ra,
                    SoGioLam = QuyTacChamCong.TinhSoGioLam(vao, ra),
                    TrangThai = QuyTacChamCong.TrangThaiKhiRa(vao, ra)
                });
            }
        }
        context.SaveChanges();
    }

    private static void BoSungChamCongHomNay(NamSaoDbContext context)
    {
        var homNay = DateTime.Today;
        var bayGio = DateTime.Now;
        if (!QuyTacChamCong.LaNgayLamViec(homNay) || bayGio.TimeOfDay < new TimeSpan(7, 40, 0)) return;

        var demo = context.Users.Where(u => u.Username == DemoNhanVien).Select(u => u.UserID).FirstOrDefault();
        var nhanViens = context.NhanViens.Where(n => n.UserID != demo).ToList();
        var daCo = context.ChamCongs.Where(c => c.NgayChamCong == homNay).Select(c => c.MaNV).ToList();
        if (nhanViens.Any(n => daCo.Contains(n.MaNV))) return;

        var rnd = new Random(homNay.DayOfYear);
        foreach (var nv in nhanViens)
        {
            if (rnd.NextDouble() < 0.1) continue; // chưa đến / nghỉ
            var phut = rnd.NextDouble() < 0.8 ? rnd.Next(7 * 60 + 35, 8 * 60 + 15) : rnd.Next(8 * 60 + 16, 9 * 60 + 5);
            var vao = homNay.AddMinutes(phut);
            if (vao > bayGio) continue;
            var cc = new ChamCong { MaNV = nv.MaNV, NgayChamCong = homNay, GioCheckIn = vao, TrangThai = QuyTacChamCong.TrangThaiKhiVao(vao) };
            if (bayGio.TimeOfDay > new TimeSpan(17, 30, 0))
            {
                var ra = homNay.AddMinutes(rnd.Next(17 * 60, 17 * 60 + 30));
                cc.GioCheckOut = ra;
                cc.SoGioLam = QuyTacChamCong.TinhSoGioLam(vao, ra);
                cc.TrangThai = QuyTacChamCong.TrangThaiKhiRa(vao, ra);
            }
            context.ChamCongs.Add(cc);
        }
        context.SaveChanges();
    }

    private static string TaoUsername(string hoTen, HashSet<string> daDung)
    {
        var parts = BoDau(hoTen).ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var goc = parts[^1] + "." + string.Concat(parts[..^1].Select(p => p[0]));
        var ten = goc;
        for (int i = 2; daDung.Contains(ten); i++) ten = goc + i;
        return ten;
    }

    private static string BoDau(string s)
    {
        var sb = new StringBuilder();
        foreach (var ch in s.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        return sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }
}
