using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;
using NamSaoHRM.Services;
using NamSaoHRM.ViewModels;

namespace NamSaoHRM.Controllers;

[Authorize]
public class ChamCongController : BaseController
{
    private readonly NamSaoDbContext _context;

    public ChamCongController(NamSaoDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var vm = new ChamCongIndexViewModel();
        if (MaNVHienTai is int maNV)
        {
            vm.NhanVien = await _context.NhanViens.Include(n => n.PhongBan).FirstOrDefaultAsync(n => n.MaNV == maNV);
            var homNay = DateTime.Today;
            vm.HomNay = await _context.ChamCongs.FirstOrDefaultAsync(c => c.MaNV == maNV && c.NgayChamCong == homNay);
            vm.GanDay = await _context.ChamCongs
                .Where(c => c.MaNV == maNV && c.NgayChamCong < homNay)
                .OrderByDescending(c => c.NgayChamCong)
                .Take(5)
                .ToListAsync();
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn()
    {
        if (MaNVHienTai is not int maNV) return RedirectToAction(nameof(Index));
        var homNay = DateTime.Today;
        if (await _context.ChamCongs.AnyAsync(c => c.MaNV == maNV && c.NgayChamCong == homNay))
        {
            ThongBao("Bạn đã check-in trong ngày hôm nay.", true);
            return RedirectToAction(nameof(Index));
        }

        var bayGio = DateTime.Now;   // Lấy giờ máy chủ, không lấy giờ máy khách
        var trangThai = QuyTacChamCong.TrangThaiKhiVao(bayGio);
        _context.ChamCongs.Add(new ChamCong
        {
            MaNV = maNV,
            NgayChamCong = homNay,
            GioCheckIn = bayGio,
            TrangThai = trangThai
        });
        await _context.SaveChangesAsync();
        ThongBao($"Check-in thành công lúc {bayGio:HH:mm} – {trangThai}.", trangThai == QuyTacChamCong.DiMuon);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut()
    {
        if (MaNVHienTai is not int maNV) return RedirectToAction(nameof(Index));
        var homNay = DateTime.Today;
        var cc = await _context.ChamCongs.FirstOrDefaultAsync(c => c.MaNV == maNV && c.NgayChamCong == homNay);
        if (cc?.GioCheckIn == null)
        {
            ThongBao("Bạn cần check-in trước khi check-out.", true);
            return RedirectToAction(nameof(Index));
        }
        if (cc.GioCheckOut != null)
        {
            ThongBao("Bạn đã check-out trong ngày hôm nay.", true);
            return RedirectToAction(nameof(Index));
        }

        var bayGio = DateTime.Now;
        cc.GioCheckOut = bayGio;
        cc.SoGioLam = QuyTacChamCong.TinhSoGioLam(cc.GioCheckIn.Value, bayGio);
        cc.TrangThai = QuyTacChamCong.TrangThaiKhiRa(cc.GioCheckIn.Value, bayGio);
        await _context.SaveChangesAsync();
        ThongBao($"Check-out thành công lúc {bayGio:HH:mm}. Số giờ làm: {cc.SoGioLam:0.##} giờ.",
            QuyTacChamCong.LaVeSom(bayGio));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> LichSu(int? thang, int? nam, int? maNV)
    {
        var homNay = DateTime.Today;
        var vm = new LichSuViewModel
        {
            Thang = thang is >= 1 and <= 12 ? thang.Value : homNay.Month,
            Nam = nam ?? homNay.Year,
            LaAdmin = LaAdmin
        };

        if (LaAdmin)
        {
            vm.DanhSachNhanVien = await _context.NhanViens.OrderBy(n => n.HoTen).ToListAsync();
            vm.MaNV = maNV ?? MaNVHienTai ?? vm.DanhSachNhanVien.FirstOrDefault()?.MaNV ?? 0;
        }
        else
        {
            vm.MaNV = MaNVHienTai ?? 0;   // Nhân viên chỉ xem được dữ liệu của mình
        }

        vm.NhanVien = await _context.NhanViens.Include(n => n.PhongBan).FirstOrDefaultAsync(n => n.MaNV == vm.MaNV);
        var tuNgay = new DateTime(vm.Nam, vm.Thang, 1);
        var denNgay = tuNgay.AddMonths(1);
        var records = await _context.ChamCongs
            .Where(c => c.MaNV == vm.MaNV && c.NgayChamCong >= tuNgay && c.NgayChamCong < denNgay)
            .ToListAsync();

        var ngayCuoi = denNgay.AddDays(-1) < homNay ? denNgay.AddDays(-1) : homNay;
        for (var d = ngayCuoi; d >= tuNgay; d = d.AddDays(-1))
        {
            if (vm.NhanVien != null && d < vm.NhanVien.NgayVaoLam) break;
            vm.Dong.Add(new DongLichSu { Ngay = d, ChamCong = records.FirstOrDefault(c => c.NgayChamCong == d) });
        }

        vm.TongNgayCong = records.Sum(QuyTacChamCong.NgayCong);
        vm.SoLanDiMuon = records.Count(QuyTacChamCong.LaDiMuon);
        vm.SoLanVeSom = records.Count(QuyTacChamCong.LaVeSom);
        vm.TongGioLam = records.Sum(c => c.SoGioLam ?? 0);
        return View(vm);
    }
}
