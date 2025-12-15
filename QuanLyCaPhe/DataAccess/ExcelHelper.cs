using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; // Thư viện dùng để "soi" code

namespace QuanLyCaPhe.Helpers
{
    public class ExcelHelper
    {
        // =============================================================
        // 1. EXPORT GENERIC (XUẤT RA EXCEL)
        // =============================================================
        public static bool ExportList<T>(string filePath, List<T> data, string sheetName)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    PropertyInfo[] properties = typeof(T).GetProperties();

                    // --- HEADER ---
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = properties[i].Name;
                    }

                    // Format Header
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
                    headerRow.Style.Font.FontColor = XLColor.White;

                    // --- DATA ---
                    for (int r = 0; r < data.Count; r++)
                    {
                        for (int c = 0; c < properties.Length; c++)
                        {
                            var value = properties[c].GetValue(data[r]);
                            // Xử lý ngày tháng hoặc null
                            if (value != null)
                            {
                                if (value is DateTime date)
                                    worksheet.Cell(r + 2, c + 1).Value = date.ToString("yyyy-MM-dd HH:mm");
                                else
                                    worksheet.Cell(r + 2, c + 1).Value = value.ToString();
                            }
                        }
                    }
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch { return false; }
        }

        // =============================================================
        // 2. IMPORT GENERIC (NHẬP TỪ EXCEL) - HÀM BẠN ĐANG CẦN
        // =============================================================
        public static List<T> ImportList<T>(string filePath) where T : new()
        {
            var list = new List<T>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên

                // Lấy dòng Header (Dòng 1) để biết cột nào tên là gì
                var headerRow = worksheet.Row(1);
                var properties = typeof(T).GetProperties();

                // Tạo map: Tên cột -> Số thứ tự cột (Ví dụ: "ProName" -> Cột 2)
                var columnMap = new Dictionary<string, int>();
                foreach (var cell in headerRow.CellsUsed())
                {
                    columnMap[cell.GetString()] = cell.Address.ColumnNumber;
                }

                // Duyệt qua các dòng dữ liệu (Bỏ dòng 1)
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    T item = new T(); // Tạo mới đối tượng (User, Product...)

                    foreach (var prop in properties)
                    {
                        // Nếu trong Excel có cột trùng tên với thuộc tính này
                        if (columnMap.ContainsKey(prop.Name))
                        {
                            int colIndex = columnMap[prop.Name];
                            var cellValue = row.Cell(colIndex).GetValue<string>(); // Lấy giá trị chuỗi

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                try
                                {
                                    // Chuyển đổi kiểu dữ liệu (String -> Int/Decimal/Double...)
                                    // Hàm ChangeTypeSafe được viết ở dưới
                                    object safeValue = ChangeTypeSafe(cellValue, prop.PropertyType);

                                    // Gán giá trị vào đối tượng
                                    prop.SetValue(item, safeValue);
                                }
                                catch
                                {
                                    // Nếu lỗi convert (vd: chữ vào số) thì bỏ qua, để mặc định 
                                }
                            }
                        }
                    }
                    list.Add(item);
                }
            }
            return list;
        }

        // Hàm phụ trợ: Chuyển đổi kiểu dữ liệu an toàn
        private static object ChangeTypeSafe(string value, Type conversionType)
        {
            // Xử lý Nullable (int?, double?...)
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value)) return null;
                conversionType = Nullable.GetUnderlyingType(conversionType);
            }

            // Xử lý riêng cho Decimal (Tiền tệ) vì Excel hay lưu dưới dạng Double
            if (conversionType == typeof(decimal))
            {
                return Convert.ToDecimal(double.Parse(value));
            }

            return Convert.ChangeType(value, conversionType);
        }
    }
}