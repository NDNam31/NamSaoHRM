using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;
using NamSaoHRM.Services;
using NamSaoHRM.ViewModels;

namespace NamSaoHRM.Controllers;

[Authorize(Roles = VaiTro.Admin)]   // Chỉ Admin mới được truy cập
public class NhanVienController : BaseController
{
    private readonly NamSaoDbContext _context;
    private readonly ILogger<NhanVienController> _logger;

    public NhanVienController(NamSaoDbContext context,
                              ILogger<NhanVienController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /NhanVien?q=...&phongBan=...&page=...
    public async Task<IActionResult> Index(string? q, int? phongBan, int page = 1)
    {
        var query = LocNhanVien(q, phongBan);
        var vm = new NhanVienListViewModel
        {
            Q = q,
            PhongBan = phongBan,
            PhongBans = await _context.PhongBans.OrderBy(p => p.MaPhongBan).ToListAsync(),
            TotalItems = await query.CountAsync()
        };
        vm.Page = Math.Clamp(page, 1, vm.TotalPages);
        vm.Items = await query
            .OrderBy(n => n.MaNV)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();
        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        await NapPhongBan();
        return View(new NhanVienCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NhanVienCreateViewModel model)
    {
        // Kiểm tra tên đăng nhập bắt buộc khi tạo mới
        if (string.IsNullOrWhiteSpace(model.Username))
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập không được bỏ trống.");
        KiemTraTuoi(model);

        // Kiểm tra trùng lặp dữ liệu
        if (!string.IsNullOrWhiteSpace(model.Email) &&
            await _context.NhanViens.AnyAsync(x => x.Email == model.Email.Trim()))
            ModelState.AddModelError(nameof(model.Email),
                "Email đã tồn tại trong hệ thống.");
        if (!string.IsNullOrWhiteSpace(model.Username) &&
            await _context.Users.AnyAsync(x => x.Username == model.Username.Trim()))
            ModelState.AddModelError(nameof(model.Username),
                "Tên đăng nhập đã được sử dụng.");

        if (!ModelState.IsValid)
        {
            await NapPhongBan();
            return View(model);
        }

        // Tạo tài khoản và hồ sơ trong cùng một giao dịch
        await using var transaction =
            await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                Username = model.Username!.Trim(),
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(model.MatKhauMacDinh),
                Role = VaiTro.NhanVien,
                TrangThai = true,
                NgayTao = DateTime.Now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var nhanVien = new NhanVien { UserID = user.UserID };
            GanDuLieu(nhanVien, model);
            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            ThongBao($"Đã thêm nhân viên {nhanVien.HoTen} thành công.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();   // Hoàn tác nếu có lỗi
            _logger.LogError(ex, "Lỗi thêm nhân viên {Email}", model.Email);
            ModelState.AddModelError(string.Empty,
                "Đã xảy ra lỗi, vui lòng thử lại.");
            await NapPhongBan();
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var nv = await _context.NhanViens.Include(n => n.User).FirstOrDefaultAsync(n => n.MaNV == id);
        if (nv == null) return NotFound();

        await NapPhongBan();
        return View(new NhanVienCreateViewModel
        {
            MaNV = nv.MaNV,
            HoTen = nv.HoTen,
            NgaySinh = nv.NgaySinh,
            GioiTinh = nv.GioiTinh,
            Email = nv.Email,
            SoDienThoai = nv.SoDienThoai,
            MaPhongBan = nv.MaPhongBan,
            ChucVu = nv.ChucVu,
            LuongCoBan = nv.LuongCoBan,
            NgayVaoLam = nv.NgayVaoLam,
            Username = nv.User?.Username
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NhanVienCreateViewModel model)
    {
        var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaNV == id);
        if (nv == null) return NotFound();

        KiemTraTuoi(model);
        if (!string.IsNullOrWhiteSpace(model.Email) &&
            await _context.NhanViens.AnyAsync(x => x.Email == model.Email.Trim() && x.MaNV != id))
            ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại trong hệ thống.");

        if (!ModelState.IsValid)
        {
            model.MaNV = id;
            await NapPhongBan();
            return View(model);
        }

        GanDuLieu(nv, model);
        await _context.SaveChangesAsync();
        ThongBao($"Đã cập nhật thông tin nhân viên {nv.HoTen}.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaNV == id);
        if (nv == null) return NotFound();
        if (nv.UserID == UserIdHienTai)
        {
            ThongBao("Không thể xóa hồ sơ gắn với tài khoản đang đăng nhập.", true);
            return RedirectToAction(nameof(Index));
        }

        var user = nv.UserID == null ? null : await _context.Users.FirstOrDefaultAsync(u => u.UserID == nv.UserID);
        _context.NhanViens.Remove(nv);   // Dữ liệu chấm công, bảng lương xóa theo (ON DELETE CASCADE)
        if (user != null) _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        ThongBao($"Đã xóa nhân viên {nv.HoTen}.");
        return RedirectToAction(nameof(Index));
    }

    // GET: /NhanVien/ExportExcel
    public async Task<IActionResult> ExportExcel(string? q, int? phongBan)
    {
        var ds = await LocNhanVien(q, phongBan).OrderBy(n => n.MaNV).ToListAsync();
        var dong = ds.Select((n, i) => new object?[]
        {
            i + 1, n.MaHienThi, n.HoTen, n.NgaySinh, n.GioiTinh, n.PhongBan?.TenPhongBan,
            n.ChucVu, n.Email, n.SoDienThoai, n.LuongCoBan, n.NgayVaoLam
        });
        var file = ExcelHelper.TaoFile("Danh sách nhân viên",
            new[] { "STT", "Mã NV", "Họ và tên", "Ngày sinh", "Giới tính", "Phòng ban", "Chức vụ", "Email", "Số điện thoại", "Lương cơ bản", "Ngày vào làm" },
            dong, 10);
        return File(file, ExcelHelper.ContentType, $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    private IQueryable<NhanVien> LocNhanVien(string? q, int? phongBan)
    {
        var query = _context.NhanViens.Include(n => n.PhongBan).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var tuKhoa = q.Trim();
            query = query.Where(n => n.HoTen.Contains(tuKhoa) || n.Email.Contains(tuKhoa));
        }
        if (phongBan.HasValue) query = query.Where(n => n.MaPhongBan == phongBan);
        return query;
    }

    private async Task NapPhongBan() =>
        ViewBag.PhongBans = new SelectList(await _context.PhongBans.OrderBy(p => p.MaPhongBan).ToListAsync(),
            "MaPhongBan", "TenPhongBan");

    private void KiemTraTuoi(NhanVienCreateViewModel model)
    {
        if (model.NgaySinh.HasValue && model.NgaySinh.Value.AddYears(18) > DateTime.Today)
            ModelState.AddModelError(nameof(model.NgaySinh), "Nhân viên phải đủ 18 tuổi.");
    }

    private static void GanDuLieu(NhanVien nv, NhanVienCreateViewModel m)
    {
        nv.HoTen = m.HoTen.Trim();
        nv.NgaySinh = m.NgaySinh!.Value;
        nv.GioiTinh = m.GioiTinh;
        nv.Email = m.Email.Trim();
        nv.SoDienThoai = m.SoDienThoai;
        nv.MaPhongBan = m.MaPhongBan!.Value;
        nv.ChucVu = m.ChucVu;
        nv.LuongCoBan = m.LuongCoBan!.Value;
        nv.NgayVaoLam = m.NgayVaoLam!.Value;
    }
}
