using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NamSaoHRM.Models;

/// <summary>Bản ghi chấm công của một nhân viên trong một ngày.</summary>
public class ChamCong
{
    [Key]
    public int MaChamCong { get; set; }

    public int MaNV { get; set; }

    [Column(TypeName = "date")]
    public DateTime NgayChamCong { get; set; }

    public DateTime? GioCheckIn { get; set; }

    public DateTime? GioCheckOut { get; set; }

    public decimal? SoGioLam { get; set; }

    [StringLength(30)]
    public string? TrangThai { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    public NhanVien? NhanVien { get; set; }
}
