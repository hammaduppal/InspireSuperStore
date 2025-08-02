using System.Collections;
using System.Drawing;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace MarketBal.Helper.Excel
{
    
    public class ExcelHandler
    {
        private readonly string _licenseOwnerName;

        public ExcelHandler(string licenseOwnerName)
        {
            _licenseOwnerName = licenseOwnerName ?? throw new ArgumentNullException(nameof(licenseOwnerName));
            // Initialize EPPlus license once per instance (EPPlus 8+)
            ExcelPackage.License.SetNonCommercialPersonal(_licenseOwnerName);
        }

        /// <summary>
        /// Builds a workbook with multiple sheets from the provided data dictionary.
        /// Key = sheet name; Value = enumerable of POCOs for that sheet.
        /// </summary>
        public async Task<byte[]> BuildWorkbook(Dictionary<string, IEnumerable> sheetData)
        {
            if (sheetData == null || sheetData.Count == 0)
                throw new ArgumentException("sheetData must contain at least one sheet.", nameof(sheetData));

            // Offload synchronous EPPlus work to background thread
            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var sheetErrors = new List<string>();

                foreach (var kv in sheetData)
                {
                    string rawName = kv.Key ?? "Sheet";
                    string sheetName = MakeValidSheetName(rawName);

                    // Ensure uniqueness
                    int suffix = 1;
                    string uniqueName = sheetName;
                    while (seenNames.Contains(uniqueName))
                    {
                        uniqueName = sheetName.Length > 25
                            ? sheetName.Substring(0, 25 - suffix.ToString().Length) + $"_{suffix}"
                            : $"{sheetName}_{suffix}";
                        suffix++;
                    }
                    seenNames.Add(uniqueName);

                    try
                    {
                        var items = kv.Value?.Cast<object>().ToList() ?? new List<object>();
                        var worksheet = package.Workbook.Worksheets.Add(uniqueName);

                        if (items.Count == 0 || items.All(i => i == null))
                        {
                            continue; // blank sheet
                        }

                        var firstNonNull = items.First(i => i != null);
                        var props = firstNonNull.GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead)
                            .ToList();

                        if (!props.Any())
                            continue;

                        // Header
                        for (int c = 0; c < props.Count; c++)
                        {
                            worksheet.Cells[1, c + 1].Value = props[c].Name;
                        }

                        using (var headerRange = worksheet.Cells[1, 1, 1, props.Count])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }

                        // Data rows
                        for (int r = 0; r < items.Count; r++)
                        {
                            var item = items[r];
                            if (item == null) continue;

                            for (int c = 0; c < props.Count; c++)
                            {
                                var prop = props[c];
                                object val;
                                try
                                {
                                    val = prop.GetValue(item);
                                }
                                catch
                                {
                                    val = null;
                                }

                                if (val != null)
                                {
                                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                    if (type.IsEnum)
                                        worksheet.Cells[r + 2, c + 1].Value = val.ToString();
                                    else if (type == typeof(bool))
                                        worksheet.Cells[r + 2, c + 1].Value = ((bool)val) ? "TRUE" : "FALSE";
                                    else if (type == typeof(DateTime))
                                        worksheet.Cells[r + 2, c + 1].Value = ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss");
                                    else
                                        worksheet.Cells[r + 2, c + 1].Value = val;
                                }
                                else
                                {
                                    worksheet.Cells[r + 2, c + 1].Value = null;
                                }
                            }
                        }

                        // Styling all data (English / LTR)
                        int totalRows = items.Count + 1; // header + data
                        using (var dataRange = worksheet.Cells[1, 1, totalRows, props.Count])
                        {
                            dataRange.Style.Font.Name = "Calibri";
                            dataRange.Style.Font.Size = 11;
                            dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        // Auto-fit and cap width
                        for (int c = 1; c <= props.Count; c++)
                        {
                            worksheet.Column(c).AutoFit();
                            if (worksheet.Column(c).Width > 50)
                                worksheet.Column(c).Width = 50;
                        }

                        worksheet.View.FreezePanes(2, 1);
                    }
                    catch (Exception ex)
                    {
                        sheetErrors.Add($"Sheet '{rawName}' failed: {ex.Message}");
                    }
                }

                if (sheetErrors.Any())
                {
                    var errSheet = package.Workbook.Worksheets.Add("Errors");
                    errSheet.Cells[1, 1].Value = "Sheet Name";
                    errSheet.Cells[1, 2].Value = "Error";

                    using (var header = errSheet.Cells[1, 1, 1, 2])
                    {
                        header.Style.Font.Bold = true;
                        header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        header.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }

                    for (int i = 0; i < sheetErrors.Count; i++)
                    {
                        errSheet.Cells[i + 2, 1].Value = sheetErrors[i].Split('\'').Length > 1
                            ? sheetErrors[i].Split('\'')[1]
                            : "Unknown";
                        errSheet.Cells[i + 2, 2].Value = sheetErrors[i];
                    }

                    errSheet.Column(1).AutoFit();
                    errSheet.Column(2).AutoFit();
                }

                return package.GetAsByteArray();
            });
        }

        private static string MakeValidSheetName(string name)
        {
            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            foreach (var c in invalid)
                name = name.Replace(c, '_');
            if (name.Length > 31)
                name = name[..31];
            return string.IsNullOrWhiteSpace(name) ? "Sheet" : name;
        }
    }

}
