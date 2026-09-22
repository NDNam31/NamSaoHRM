using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;
using NamSaoHRM.Services;
using NamSaoHRM.ViewModels;

namespace NamSaoHRM.Controllers;

[Authorize(Roles = VaiTro.Admin)]
public class DashboardController : BaseController
{
    private readonly NamSaoDbContext _context;

    public DashboardController(NamSaoDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var homNay = DateTime.Today;
        var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
        var tu7Ngay = homNay.AddDays(-13);

        var nhanViens = await _context.NhanViens.Include(n => n.PhongBan).ToListAsync();
        var phongBans = await _context.PhongBans.ToListAsync();
        var chamCongHomNay = await _context.ChamCongs
            .Include(c => c.NhanVien)
            .Where(c => c.NgayChamCong == homNay)
            .ToListAsync();
        var chamCongGanDay = await _context.ChamCongs
            .Where(c => c.NgayChamCong >= tu7Ngay && c.NgayChamCong <= homNay)
            .ToListAsync();
        var chamCongThang = chamCongGanDay.Count(c => c.NgayChamCong >= dauThang && QuyTacChamCong.LaDiMuon(c));
        if (dauThang < tu7Ngay)
            chamCongThang = (await _context.ChamCongs
                .Where(c => c.NgayChamCong >= dauThang && c.NgayChamCong <= homNay)
                .ToListAsync()).Count(QuyTacChamCong.LaDiMuon);

        var ngayLamViec = Enumerable.Range(0, 14).Select(i => tu7Ngay.AddDays(i))
            .Where(QuyTacChamCong.LaNgayLamViec).ToList();

        var vm = new DashboardViewModel
        {
            TongNhanVien = nhanViens.Count,
            SoPhongBan = phongBans.Count,
            DaCheckInHomNay = chamCongHomNay.Count(c => c.GioCheckIn != null),
            DiMuonHomNay = chamCongHomNay.Count(QuyTacChamCong.LaDiMuon),
            DiMuonThangNay = chamCongThang,
            TaiKhoanBiKhoa = await _context.Users.CountAsync(u => !u.TrangThai),
            NhanSuTheoPhongBan = phongBans
                .Select(p => new PhongBanThongKe(p.TenPhongBan.Replace("Phòng ", ""), nhanViens.Count(n => n.MaPhongBan == p.MaPhongBan)))
                .ToList(),
            ChamCongHomNay = chamCongHomNay
                .OrderByDescending(c => c.GioCheckOut ?? c.GioCheckIn)
                .Take(8).ToList(),
            NhanNgay = ngayLamViec.Select(d => d.ToString("dd/MM")).ToArray(),
            DiLamTheoNgay = ngayLamViec.Select(d => chamCongGanDay.Count(c => c.NgayChamCong == d)).ToArray()
        };
        foreach (var c in vm.ChamCongHomNay)
            c.NhanVien!.PhongBan = phongBans.FirstOrDefault(p => p.MaPhongBan == c.NhanVien.MaPhongBan);

        return View(vm);
    }
}
