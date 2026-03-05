using System.Data;
using ClosedXML.Excel;

namespace DbQueryExplorer.Services;

public static class ExcelExportService
{
    public static void Export(DataTable data, string filePath)
    {
        // Pre-create colors once — avoids repeated string parsing inside loops
        var headerBg = XLColor.FromHtml("#4472C4");
        var headerFg = XLColor.White;
        var zebraBg  = XLColor.FromHtml("#EBF0FA");

        int colCount = data.Columns.Count;
        int rowCount = data.Rows.Count;

        // Convert rows to object[][] — replace DBNull with Blank so typed
        // DataTable columns (e.g. DateTime) don't throw on empty cells.
        var rows = new object[rowCount][];
        for (int r = 0; r < rowCount; r++)
        {
            var src = data.Rows[r].ItemArray;
            var dest = new object[colCount];
            for (int c = 0; c < colCount; c++)
                dest[c] = src[c] == DBNull.Value ? string.Empty : src[c]!;
            rows[r] = dest;
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Sonuçlar");

        // ── Header row ──────────────────────────────────────────────────────
        for (int col = 0; col < colCount; col++)
        {
            var cell = ws.Cell(1, col + 1);
            cell.Value = data.Columns[col].ColumnName;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = headerBg;
            cell.Style.Font.FontColor = headerFg;
        }

        // ── Bulk insert data rows (dramatically faster than cell-by-cell) ───
        ws.Cell(2, 1).InsertData(rows);

        // ── Zebra striping: row-level range, not cell-by-cell ───────────────
        for (int row = 0; row < rowCount; row += 2)
        {
            ws.Range(row + 2, 1, row + 2, colCount)
              .Style.Fill.BackgroundColor = zebraBg;
        }

        // ── Column widths: fixed reasonable max instead of AdjustToContents ─
        // AdjustToContents iterates every cell — catastrophically slow on 800K rows.
        foreach (var col in ws.ColumnsUsed())
            col.Width = Math.Min(col.Width, 40);

        ws.SheetView.FreezeRows(1);

        workbook.SaveAs(filePath);
    }
}
