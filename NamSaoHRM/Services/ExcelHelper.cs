using ClosedXML.Excel;

namespace NamSaoHRM.Services;

/// <summary>Tạo file Excel (.xlsx) từ dữ liệu dạng bảng bằng thư viện ClosedXML.</summary>
public static class ExcelHelper
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <param name="cotSo">Chỉ số (bắt đầu từ 1) các cột định dạng số có phân cách hàng nghìn.</param>
    public static byte[] TaoFile(string tieuDe, string[] tieuDeCot, IEnumerable<object?[]> dong, params int[] cotSo)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("BaoCao");
        int soCot = tieuDeCot.Length;

        ws.Cell(1, 1).Value = "CÔNG TY CỔ PHẦN THƯƠNG MẠI ĐIỆN TỬ NĂM SAO";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(2, 1).Value = tieuDe.ToUpper();
        ws.Range(2, 1, 2, soCot).Merge();
        ws.Range(2, 1, 2, soCot).Style.Font.Bold = true;
        ws.Range(2, 1, 2, soCot).Style.Font.FontSize = 14;
        ws.Range(2, 1, 2, soCot).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(3, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cell(3, 1).Style.Font.Italic = true;

        const int hangTieuDe = 5;
        for (int c = 0; c < soCot; c++) ws.Cell(hangTieuDe, c + 1).Value = tieuDeCot[c];
        var header = ws.Range(hangTieuDe, 1, hangTieuDe, soCot);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F3A5F");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int r = hangTieuDe;
        foreach (var d in dong)
        {
            r++;
            for (int c = 0; c < soCot && c < d.Length; c++)
            {
                var cell = ws.Cell(r, c + 1);
                switch (d[c])
                {
                    case null: break;
                    case decimal m: cell.Value = (double)m; break;
                    case int i: cell.Value = (double)i; break;
                    case double db: cell.Value = db; break;
                    case DateTime dt: cell.Value = dt.ToString("dd/MM/yyyy"); break;
                    default: cell.Value = d[c]!.ToString() ?? string.Empty; break;
                }
            }
        }

        var bang = ws.Range(hangTieuDe, 1, Math.Max(r, hangTieuDe), soCot);
        bang.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        bang.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        foreach (var c in cotSo) ws.Column(c).Style.NumberFormat.Format = "#,##0";
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
