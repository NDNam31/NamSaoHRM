using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Data;
using NamSaoHRM.Models;
using NamSaoHRM.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// Đăng ký DbContext kết nối SQL Server (chuỗi kết nối trong appsettings.json)
builder.Services.AddDbContext<NamSaoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BangLuongService>();

// Xác thực bằng Cookie, phân quyền theo vai trò (Role)
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

// Khởi tạo CSDL và dữ liệu mẫu ở lần chạy đầu tiên
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NamSaoDbContext>();
    SeedData.Initialize(context, app.Environment.IsDevelopment());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Định dạng tiếng Việt (ngày dd/MM/yyyy, số 1.000.000)
var vi = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = vi;
CultureInfo.DefaultThreadCurrentUICulture = vi;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(vi),
    SupportedCultures = new[] { vi },
    SupportedUICultures = new[] { vi }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // Xác thực: người dùng là ai?
app.UseAuthorization();    // Phân quyền: người dùng được làm gì?

app.MapControllerRoute(name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");
app.Run();
