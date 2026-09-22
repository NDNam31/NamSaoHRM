using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NamSaoHRM.Models;

/// <summary>Phòng ban trong công ty.</summary>
public class PhongBan
{
    [Key]
    public int MaPhongBan { get; set; }

    [Required(ErrorMessage = "Tên phòng ban không được bỏ trống.")]
    [StringLength(100)]
    public string TenPhongBan { get; set; } = string.Empty;

    [StringLength(255)]
    public string? MoTa { get; set; }

    [Column(TypeName = "date")]
    public DateTime? NgayThanhLap { get; set; }

    public ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();

    [NotMapped]
    public string MaHienThi => $"PB{MaPhongBan:D2}";
}
