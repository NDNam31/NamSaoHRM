using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;
using NamSaoHRM.ViewModels;

namespace NamSaoHRM.Controllers;

/// <summary>Màn hình Phòng ban &amp; Tài khoản (2 tab).</summary>
[Authorize(Roles = VaiTro.Admin)]
public class PhongBanController : BaseController
{
    private readonly NamSaoDbContext _context;

    public PhongBanController(NamSaoDbContext context) => _context = context;

    public async Task<IActionResult> Index(string tab = "phongban")
    {
        var phongBans = await _context.PhongBans.OrderBy(p => p.MaPhongBan).ToListAsync();
        var nhanViens = await _context.NhanViens.ToListAsync();
        var vm = new PhongBanTaiKhoanViewModel
        {
            Tab = tab == "taikhoan" ? "taikhoan" : "phongban",
            PhongBans = phongBans.Select(p => new PhongBanDong
            {
                PhongBan = p,
                SoNhanVien = nhanViens.Count(n => n.MaPhongBan == p.MaPhongBan)
            }).ToList(),
            TaiKhoans = await _context.Users.Include(u => u.NhanVien)
                .OrderBy(u => u.Role).ThenBy(u => u.UserID).ToListAsync(),
            UserIdHienTai = UserIdHienTai
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string tenPhongBan, string? moTa)
    {
        if (string.IsNullOrWhiteSpace(tenPhongBan))
            ThongBao("Tên phòng ban không được bỏ trống.", true);
        else if (await _context.PhongBans.AnyAsync(p => p.TenPhongBan == tenPhongBan.Trim()))
            ThongBao("Tên phòng ban đã tồn tại.", true);
        else
        {
            _context.PhongBans.Add(new PhongBan { TenPhongBan = tenPhongBan.Trim(), MoTa = moTa, NgayThanhLap = DateTime.Today });
            await _context.SaveChangesAsync();
            ThongBao($"Đã thêm phòng ban \"{tenPhongBan.Trim()}\".");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string tenPhongBan, string? moTa)
    {
        var pb = await _context.PhongBans.FirstOrDefaultAsync(p => p.MaPhongBan == id);
        if (pb == null) return NotFound();
        if (string.IsNullOrWhiteSpace(tenPhongBan))
            ThongBao("Tên phòng ban không được bỏ trống.", true);
        else if (await _context.PhongBans.AnyAsync(p => p.TenPhongBan == tenPhongBan.Trim() && p.MaPhongBan != id))
            ThongBao("Tên phòng ban đã tồn tại.", true);
        else
        {
            pb.TenPhongBan = tenPhongBan.Trim();
            pb.MoTa = moTa;
            await _context.SaveChangesAsync();
            ThongBao("Đã cập nhật phòng ban.");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var pb = await _context.PhongBans.FirstOrDefaultAsync(p => p.MaPhongBan == id);
        if (pb == null) return NotFound();
        var soNV = await _context.NhanViens.CountAsync(n => n.MaPhongBan == id);
        if (soNV > 0)
        {
            ThongBao($"Không thể xóa \"{pb.TenPhongBan}\" vì vẫn còn {soNV} nhân viên trực thuộc.", true);
            return RedirectToAction(nameof(Index));
        }
        _context.PhongBans.Remove(pb);
        await _context.SaveChangesAsync();
        ThongBao($"Đã xóa phòng ban \"{pb.TenPhongBan}\".");
        return RedirectToAction(nameof(Index));
    }
}
