using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;
using NamSaoHRM.Services;
using NamSaoHRM.ViewModels;

namespace NamSaoHRM.Controllers;

[Authorize]
public class BangLuongController : BaseController
{
    private readonly NamSaoDbContext _context;
    private readonly BangLuongService _service;

    public BangLuongController(NamSaoDbContext context, BangLuongService service)
    {
        _context = context;
        _service = service;
    }

    public async Task<IActionResult> Index(int? thang, int? nam)
    {
        var macDinh = DateTime.Today.AddMonths(-1);
        var vm = new BangLuongViewModel
        {
            Thang = thang is >= 1 and <= 12 ? thang.Value : macDinh.Month,
            Nam = nam ?? macDinh.Year,
            LaAdmin = LaAdmin
        };
        vm.Items = await LayBangLuong(vm.Thang, vm.Nam);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = VaiTro.Admin)]
    public async Task<IActionResult> TongHop(int thang, int nam)
    {
        var soNV = await _service.TongHopAsync(thang, nam);
        ThongBao($"Đã tổng hợp bảng lương tháng {thang:00}/{nam} cho {soNV} nhân viên.");
        return RedirectToAction(nameof(Index), new { thang, nam });
    }

    [Authorize(Roles = VaiTro.Admin)]
    public async Task<IActionResult> XuatExcel(int thang, int nam)
    {
        var ds = await LayBangLuong(thang, nam);
        var dong = ds.Select((b, i) => new object?[]
        {
            i + 1, b.NhanVien?.MaHienThi, b.NhanVien?.HoTen, b.NhanVien?.PhongBan?.TenPhongBan,
            b.NhanVien?.LuongCoBan, b.SoNgayCong, b.SoLanDiMuon, b.PhuCap, b.KhauTru, b.TongLuong
        });
        var file = ExcelHelper.TaoFile($"Bảng lương tháng {thang:00}/{nam}",
            new[] { "STT", "Mã NV", "Họ và tên", "Phòng ban", "Lương cơ bản", "Ngày công", "Số lần đi muộn", "Phụ cấp", "Phạt đi muộn", "Thực lĩnh" },
            dong, 5, 8, 9, 10);
        return File(file, ExcelHelper.ContentType, $"BangLuong_{nam}_{thang:00}.xlsx");
    }

    private async Task<List<BangLuong>> LayBangLuong(int thang, int nam)
    {
        var query = _context.BangLuongs.Include(b => b.NhanVien).Where(b => b.Thang == thang && b.Nam == nam);
        if (!LaAdmin)
        {
            var maNV = MaNVHienTai ?? 0;
            query = query.Where(b => b.MaNV == maNV);   // Nhân viên chỉ xem bảng lương của mình
        }
        var ds = await query.OrderBy(b => b.MaNV).ToListAsync();
        var phongBans = await _context.PhongBans.ToListAsync();
        foreach (var b in ds)
            if (b.NhanVien != null) b.NhanVien.PhongBan ??= phongBans.FirstOrDefault(p => p.MaPhongBan == b.NhanVien.MaPhongBan);
        return ds;
    }
}
