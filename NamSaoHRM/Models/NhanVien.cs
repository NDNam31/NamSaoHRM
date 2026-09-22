using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NamSaoHRM.Models;

/// <summary>Hồ sơ nhân viên.</summary>
public class NhanVien
{
    [Key]
    public int MaNV { get; set; }

    public int? UserID { get; set; }

    public int MaPhongBan { get; set; }

    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime NgaySinh { get; set; }

    [Required, StringLength(10)]
    public string GioiTinh { get; set; } = "Nam";

    [Required, Column(TypeName = "varchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Column(TypeName = "varchar(15)")]
    public string? SoDienThoai { get; set; }

    [StringLength(50)]
    public string? ChucVu { get; set; }

    public decimal LuongCoBan { get; set; }

    [Column(TypeName = "date")]
    public DateTime NgayVaoLam { get; set; }

    public User? User { get; set; }
    public PhongBan? PhongBan { get; set; }
    public ICollection<ChamCong> ChamCongs { get; set; } = new List<ChamCong>();
    public ICollection<BangLuong> BangLuongs { get; set; } = new List<BangLuong>();

    /// <summary>Mã hiển thị dạng NV001.</summary>
    [NotMapped]
    public string MaHienThi => $"NV{MaNV:D3}";
}
