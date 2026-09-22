using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NamSaoHRM.Models;

/// <summary>Tài khoản đăng nhập hệ thống.</summary>
public class User
{
    [Key]
    public int UserID { get; set; }

    [Required, Column(TypeName = "varchar(50)")]
    public string Username { get; set; } = string.Empty;

    [Required, Column(TypeName = "varchar(255)")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, Column(TypeName = "varchar(20)")]
    public string Role { get; set; } = VaiTro.NhanVien;

    /// <summary>true: Hoạt động, false: Bị khóa</summary>
    public bool TrangThai { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public NhanVien? NhanVien { get; set; }
}

public static class VaiTro
{
    public const string Admin = "Admin";
    public const string NhanVien = "NhanVien";

    public static string HienThi(string role) => role == Admin ? "Quản trị viên" : "Nhân viên";
}
