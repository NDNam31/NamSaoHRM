using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;
using NamSaoHRM.ViewModels;

namespace NamSaoHRM.Controllers;

public class AccountController : BaseController
{
    private readonly NamSaoDbContext _context;

    public AccountController(NamSaoDbContext context) => _context = context;

    // Trang gốc "/" chuyển tới màn hình đăng nhập
    [AllowAnonymous]
    public IActionResult Index() => RedirectToAction(nameof(Login));

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return ChuyenTheoVaiTro(User.IsInRole(VaiTro.Admin));
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        // Dữ liệu nhập trống hoặc không hợp lệ
        if (!ModelState.IsValid) return View(model);

        var user = await _context.Users.AsNoTracking()
            .Include(u => u.NhanVien)
            .FirstOrDefaultAsync(u => u.Username == model.Username.Trim());

        // So khớp mật khẩu với chuỗi băm BCrypt lưu trong CSDL
        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty,
                "Sai tên đăng nhập hoặc mật khẩu.");
            return View(model);
        }
        if (!user.TrangThai)
        {
            ModelState.AddModelError(string.Empty,
                "Tài khoản đã bị khóa, vui lòng liên hệ quản trị viên.");
            return View(model);
        }

        // Tạo danh tính người dùng kèm vai trò (Role) để phân quyền
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("HoTen", user.NhanVien?.HoTen ?? user.Username),
            new Claim("ChucVu", user.NhanVien?.ChucVu ?? VaiTro.HienThi(user.Role))
        };
        if (user.NhanVien != null) claims.Add(new Claim("MaNV", user.NhanVien.MaNV.ToString()));

        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return ChuyenTheoVaiTro(user.Role == VaiTro.Admin);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // Điều hướng theo vai trò
    private IActionResult ChuyenTheoVaiTro(bool laAdmin) =>
        laAdmin ? RedirectToAction("Index", "Dashboard") : RedirectToAction("Index", "ChamCong");
}
