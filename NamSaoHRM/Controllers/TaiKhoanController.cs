using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;

namespace NamSaoHRM.Controllers;

/// <summary>Quản lý tài khoản và phân quyền (hiển thị ở tab thứ hai của màn hình Phòng ban).</summary>
[Authorize(Roles = VaiTro.Admin)]
public class TaiKhoanController : BaseController
{
    private const string MatKhauMacDinh = "123456";
    private readonly NamSaoDbContext _context;

    public TaiKhoanController(NamSaoDbContext context) => _context = context;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaMo(int id)
    {
        var u = await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
        if (u == null) return NotFound();
        if (u.UserID == UserIdHienTai)
            ThongBao("Không thể khóa tài khoản đang đăng nhập.", true);
        else
        {
            u.TrangThai = !u.TrangThai;
            await _context.SaveChangesAsync();
            ThongBao(u.TrangThai ? $"Đã mở khóa tài khoản {u.Username}." : $"Đã khóa tài khoản {u.Username}.");
        }
        return VeTab();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiVaiTro(int id, string role)
    {
        var u = await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
        if (u == null) return NotFound();
        if (role != VaiTro.Admin && role != VaiTro.NhanVien) return BadRequest();
        if (u.UserID == UserIdHienTai)
            ThongBao("Không thể tự thay đổi vai trò của chính mình.", true);
        else
        {
            u.Role = role;
            await _context.SaveChangesAsync();
            ThongBao($"Đã đổi vai trò của {u.Username} thành {VaiTro.HienThi(role)}.");
        }
        return VeTab();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatLaiMatKhau(int id)
    {
        var u = await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
        if (u == null) return NotFound();
        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(MatKhauMacDinh);
        await _context.SaveChangesAsync();
        ThongBao($"Đã đặt lại mật khẩu của {u.Username} về mặc định ({MatKhauMacDinh}).");
        return VeTab();
    }

    private IActionResult VeTab() => RedirectToAction("Index", "PhongBan", new { tab = "taikhoan" });
}
