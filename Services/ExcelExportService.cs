using System.Data;
using ClosedXML.Excel;

namespace DbQueryExplorer.Services;

public static class ExcelExportService
{
    public static void Export(DataTable data, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Sonuçlar");

        // Header row
        for (int col = 0; col < data.Columns.Count; col++)
        {
            var cell = ws.Cell(1, col + 1);
            cell.Value = data.Columns[col].ColumnName;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        for (int row = 0; row < data.Rows.Count; row++)
        {
            for (int col = 0; col < data.Columns.Count; col++)
            {
                var rawValue = data.Rows[row][col];
                var cell = ws.Cell(row + 2, col + 1);

                if (rawValue == DBNull.Value)
                {
                    cell.Value = string.Empty;
                }
                else
                {
                    cell.Value = rawValue switch
                    {
                        bool b    => b,
                        int i     => i,
                        long l    => l,
                        double d  => d,
                        decimal m => (double)m,
                        DateTime dt => dt,
                        _         => rawValue.ToString() ?? string.Empty
                    };
                }

                // Zebra striping
                if (row % 2 == 0)
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EBF0FA");
            }
        }

        ws.Columns().AdjustToContents(1, 60);
        ws.SheetView.FreezeRows(1);

        workbook.SaveAs(filePath);
    }
}
