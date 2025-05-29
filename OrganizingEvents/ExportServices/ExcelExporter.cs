using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ExcelExporter
{
    public static byte[] ExportToExcel<T>(List<T> data)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Write column headers
            var properties = typeof(T).GetProperties()
                                     .Where(p => p.Name != "Role")
                                     .ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            // Write data
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < properties.Count; j++)
                {
                    var value = properties[j].GetValue(data[i]);
                    worksheet.Cell(i + 2, j + 1).Value = Convert.ToString(value);
                }
            }

            // Convert the workbook to a byte array
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}