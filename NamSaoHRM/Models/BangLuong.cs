using System.ComponentModel.DataAnnotations;

namespace NamSaoHRM.Models;

/// <summary>Bảng lương tháng của một nhân viên.</summary>
public class BangLuong
{
    [Key]
    public int MaBangLuong { get; set; }

    public int MaNV { get; set; }

    public int Thang { get; set; }

    public int Nam { get; set; }

    public decimal SoNgayCong { get; set; }

    public int SoLanDiMuon { get; set; }

    public decimal PhuCap { get; set; }

    public decimal KhauTru { get; set; }

    public decimal TongLuong { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public NhanVien? NhanVien { get; set; }
}
