using NamSaoHRM.Models;

namespace NamSaoHRM.Services;

/// <summary>Các quy tắc nghiệp vụ chấm công và tính lương.</summary>
public static class QuyTacChamCong
{
    public static readonly TimeSpan GioBatDau = new(8, 0, 0);
    public static readonly TimeSpan MocDiMuon = new(8, 15, 0);
    public static readonly TimeSpan GioKetThuc = new(17, 0, 0);

    public const int NgayCongChuan = 26;
    public const decimal PhatMoiLanDiMuon = 50_000m;
    public const decimal PhuCapMacDinh = 730_000m;

    public const string DungGio = "Đúng giờ";
    public const string DiMuon = "Đi muộn";
    public const string VeSom = "Về sớm";
    public const string DiMuonVeSom = "Đi muộn, về sớm";

    public static bool LaDiMuon(DateTime gioVao) => gioVao.TimeOfDay > MocDiMuon;
    public static bool LaVeSom(DateTime gioRa) => gioRa.TimeOfDay < GioKetThuc;

    public static bool LaDiMuon(ChamCong c) => c.GioCheckIn.HasValue && LaDiMuon(c.GioCheckIn.Value);
    public static bool LaVeSom(ChamCong c) => c.GioCheckOut.HasValue && LaVeSom(c.GioCheckOut.Value);

    /// <summary>Trạng thái ngay sau khi check-in.</summary>
    public static string TrangThaiKhiVao(DateTime gioVao) => LaDiMuon(gioVao) ? DiMuon : DungGio;

    /// <summary>Trạng thái tổng hợp sau khi check-out.</summary>
    public static string TrangThaiKhiRa(DateTime gioVao, DateTime gioRa)
    {
        bool muon = LaDiMuon(gioVao), som = LaVeSom(gioRa);
        return muon && som ? DiMuonVeSom : muon ? DiMuon : som ? VeSom : DungGio;
    }

    /// <summary>Số giờ làm, trừ 1 giờ nghỉ trưa nếu làm xuyên qua 12:00–13:00.</summary>
    public static decimal TinhSoGioLam(DateTime gioVao, DateTime gioRa)
    {
        var gio = (gioRa - gioVao).TotalHours;
        if (gioVao.TimeOfDay < new TimeSpan(12, 0, 0) && gioRa.TimeOfDay > new TimeSpan(13, 0, 0)) gio -= 1;
        return Math.Round((decimal)Math.Max(gio, 0), 2);
    }

    /// <summary>Ngày công: đủ check-in + check-out = 1; chỉ check-in = 0,5.</summary>
    public static decimal NgayCong(ChamCong c) =>
        c.GioCheckIn.HasValue && c.GioCheckOut.HasValue ? 1m : c.GioCheckIn.HasValue ? 0.5m : 0m;

    public static decimal TinhLuong(decimal luongCoBan, decimal soNgayCong, decimal phuCap, decimal khauTru)
    {
        var luong = Math.Round(luongCoBan / NgayCongChuan * soNgayCong, 0) + phuCap - khauTru;
        return Math.Max(luong, 0);
    }

    public static bool LaNgayLamViec(DateTime d) => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday;
}
