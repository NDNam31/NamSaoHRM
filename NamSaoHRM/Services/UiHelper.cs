using System.Globalization;

namespace NamSaoHRM.Services;

/// <summary>Các hàm hỗ trợ hiển thị trên giao diện.</summary>
public static class UiHelper
{
    private static readonly string[] MauAvatar = { "#1f5fa8", "#0f766e", "#7c3aed", "#b45309", "#be123c", "#0369a1", "#4d7c0f", "#9333ea" };

    public static string ChuCaiDau(string? hoTen)
    {
        var p = (hoTen ?? "?").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (p.Length == 0) return "?";
        return p.Length == 1 ? p[0][..1].ToUpper() : (p[0][..1] + p[^1][..1]).ToUpper();
    }

    public static string MauTheoTen(string? hoTen) =>
        MauAvatar[Math.Abs((hoTen ?? "").Aggregate(17, (h, c) => h * 31 + c)) % MauAvatar.Length];

    public static string LopTrangThai(string? trangThai) => trangThai switch
    {
        QuyTacChamCong.DungGio => "badge-soft-success",
        QuyTacChamCong.VeSom => "badge-soft-warning",
        QuyTacChamCong.DiMuon or QuyTacChamCong.DiMuonVeSom => "badge-soft-danger",
        _ => "badge-soft-secondary"
    };

    public static string Thu(DateTime d) => d.DayOfWeek switch
    {
        DayOfWeek.Monday => "Thứ Hai",
        DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư",
        DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu",
        DayOfWeek.Saturday => "Thứ Bảy",
        _ => "Chủ Nhật"
    };

    public static string Tien(decimal so) => so.ToString("N0", new CultureInfo("vi-VN")) + " ₫";
}
