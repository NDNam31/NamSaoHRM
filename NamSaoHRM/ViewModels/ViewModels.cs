using System.ComponentModel.DataAnnotations;
using NamSaoHRM.Models;

namespace NamSaoHRM.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }
}

/// <summary>Dữ liệu form Thêm mới / Cập nhật nhân viên.</summary>
public class NhanVienCreateViewModel
{
    public int MaNV { get; set; }

    [Required(ErrorMessage = "Họ tên không được bỏ trống.")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime? NgaySinh { get; set; }

    [Required]
    [Display(Name = "Giới tính")]
    public string GioiTinh { get; set; } = "Nam";

    [Required(ErrorMessage = "Email không được bỏ trống.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.")]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phòng ban.")]
    [Display(Name = "Phòng ban")]
    public int? MaPhongBan { get; set; }

    [StringLength(50)]
    [Display(Name = "Chức vụ")]
    public string? ChucVu { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lương cơ bản.")]
    [Range(1_000_000, 1_000_000_000, ErrorMessage = "Lương cơ bản phải từ 1.000.000 đến 1.000.000.000 VNĐ.")]
    [Display(Name = "Lương cơ bản (VNĐ)")]
    public decimal? LuongCoBan { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày vào làm.")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày vào làm")]
    public DateTime? NgayVaoLam { get; set; } = DateTime.Today;

    [Display(Name = "Tên đăng nhập")]
    public string? Username { get; set; }

    [Display(Name = "Mật khẩu mặc định")]
    public string MatKhauMacDinh { get; set; } = "123456";
}

public class NhanVienListViewModel
{
    public List<NhanVien> Items { get; set; } = new();
    public List<PhongBan> PhongBans { get; set; } = new();
    public string? Q { get; set; }
    public int? PhongBan { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
}

public record PhongBanThongKe(string Ten, int SoNhanVien);

public class DashboardViewModel
{
    public int TongNhanVien { get; set; }
    public int SoPhongBan { get; set; }
    public int DaCheckInHomNay { get; set; }
    public int DiMuonHomNay { get; set; }
    public int DiMuonThangNay { get; set; }
    public int TaiKhoanBiKhoa { get; set; }
    public List<PhongBanThongKe> NhanSuTheoPhongBan { get; set; } = new();
    public List<ChamCong> ChamCongHomNay { get; set; } = new();
    public int[] DiLamTheoNgay { get; set; } = Array.Empty<int>();
    public string[] NhanNgay { get; set; } = Array.Empty<string>();
}

public class PhongBanDong
{
    public PhongBan PhongBan { get; set; } = null!;
    public int SoNhanVien { get; set; }
}

public class PhongBanTaiKhoanViewModel
{
    public string Tab { get; set; } = "phongban";
    public List<PhongBanDong> PhongBans { get; set; } = new();
    public List<User> TaiKhoans { get; set; } = new();
    public int UserIdHienTai { get; set; }
}

public class ChamCongIndexViewModel
{
    public NhanVien? NhanVien { get; set; }
    public ChamCong? HomNay { get; set; }
    public List<ChamCong> GanDay { get; set; } = new();
}

public class DongLichSu
{
    public DateTime Ngay { get; set; }
    public ChamCong? ChamCong { get; set; }
    public bool CuoiTuan => !Services.QuyTacChamCong.LaNgayLamViec(Ngay);
}

public class LichSuViewModel
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public int MaNV { get; set; }
    public NhanVien? NhanVien { get; set; }
    public List<NhanVien> DanhSachNhanVien { get; set; } = new();
    public bool LaAdmin { get; set; }
    public List<DongLichSu> Dong { get; set; } = new();
    public decimal TongNgayCong { get; set; }
    public int SoLanDiMuon { get; set; }
    public int SoLanVeSom { get; set; }
    public decimal TongGioLam { get; set; }
}

public class BangLuongViewModel
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public bool LaAdmin { get; set; }
    public List<BangLuong> Items { get; set; } = new();
    public decimal TongThucLinh => Items.Sum(x => x.TongLuong);
    public decimal TongKhauTru => Items.Sum(x => x.KhauTru);
}
