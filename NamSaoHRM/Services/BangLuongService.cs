using Microsoft.EntityFrameworkCore;
using NamSaoHRM.Models;

namespace NamSaoHRM.Services;

/// <summary>Tổng hợp ngày công và tính bảng lương tháng.</summary>
public class BangLuongService
{
    private readonly NamSaoDbContext _context;

    public BangLuongService(NamSaoDbContext context) => _context = context;

    public async Task<int> TongHopAsync(int thang, int nam)
    {
        var tuNgay = new DateTime(nam, thang, 1);
        var denNgay = tuNgay.AddMonths(1);

        var nhanViens = await _context.NhanViens.ToListAsync();
        var chamCongs = await _context.ChamCongs
            .Where(c => c.NgayChamCong >= tuNgay && c.NgayChamCong < denNgay)
            .ToListAsync();
        var bangCu = await _context.BangLuongs
            .Where(b => b.Thang == thang && b.Nam == nam)
            .ToListAsync();

        foreach (var nv in nhanViens)
        {
            var cc = chamCongs.Where(c => c.MaNV == nv.MaNV).ToList();
            var soNgayCong = cc.Sum(QuyTacChamCong.NgayCong);
            var soLanDiMuon = cc.Count(QuyTacChamCong.LaDiMuon);
            var khauTru = soLanDiMuon * QuyTacChamCong.PhatMoiLanDiMuon;
            var phuCap = soNgayCong > 0 ? QuyTacChamCong.PhuCapMacDinh : 0;

            var bl = bangCu.FirstOrDefault(b => b.MaNV == nv.MaNV);
            if (bl == null)
            {
                bl = new BangLuong { MaNV = nv.MaNV, Thang = thang, Nam = nam };
                _context.BangLuongs.Add(bl);
            }
            bl.SoNgayCong = soNgayCong;
            bl.SoLanDiMuon = soLanDiMuon;
            bl.PhuCap = phuCap;
            bl.KhauTru = khauTru;
            bl.TongLuong = QuyTacChamCong.TinhLuong(nv.LuongCoBan, soNgayCong, phuCap, khauTru);
            bl.NgayTao = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return nhanViens.Count;
    }
}
