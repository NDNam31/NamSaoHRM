using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NamSaoHRM.Models;

namespace NamSaoHRM.Controllers;

public abstract class BaseController : Controller
{
    protected int UserIdHienTai => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    protected int? MaNVHienTai => int.TryParse(User.FindFirstValue("MaNV"), out var id) ? id : null;

    protected bool LaAdmin => User.IsInRole(VaiTro.Admin);

    protected void ThongBao(string noiDung, bool loi = false) =>
        TempData[loi ? "Error" : "Success"] = noiDung;
}
