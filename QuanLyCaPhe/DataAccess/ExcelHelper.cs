using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuanLyCaPhe.Models;

namespace QuanLyCaPhe.Helpers
{
    public class ExcelHelper
    {
        // =============================================================
        // HÀM DÙNG CHUNG: ĐỊNH NGHĨA TÊN CỘT Ở MỘT CHỖ DUY NHẤT
        // =============================================================
        private static Dictionary<string, string> GetHeaderMap<T>()
        {
            if (typeof(T) == typeof(Ingredient))
            {
                return new Dictionary<string, string>
                {
                    { "Id", "STT" },
                    { "IngName", "Tên nguyên liệu" },
                    { "Unit", "Đơn vị" },
                    { "Quantity", "Còn lại" }
                };
            }
            else if (typeof(T) == typeof(InputInfo))
            {
                return new Dictionary<string, string>
                {
                    { "Id", "STT" },
                    { "DateInput", "Ngày nhập" },
                    { "IngredientName", "Nguyên liệu" },
                    { "Count", "Số lượng" },
                    { "InputPrice", "Tổng tiền" },
                };
            }
            else if (typeof(T) == typeof(Bill))
            {
                return new Dictionary<string, string>
                {
                    { "Id", "STT" },
                    { "DateCheckIn", "Ngày thực hiện" },
                    { "TableName", "Bàn" },
                    { "StaffName", "Nhân viên" },
                    { "TotalPrice", "Tổng tiền" },
                };
            }
            else if (typeof(T) == typeof(Product))
            {
                return new Dictionary<string, string>
                {
                    { "Id", "STT" },
                    { "ProName", "Tên món" },
                    { "Price", "Giá" }
                };
            }
            else if (typeof(T) == typeof(User))
            {
                return new Dictionary<string, string>
                {
                    { "Id", "STT" },
                    { "FullName", "Họ và tên" },
                    { "Email", "Email" },
                    { "Phone", "Số điện thoại" },
                    { "Address", "Địa chỉ" },
                    { "Gender", "Giới tính" },
                    { "CreatedAt", "Ngày tạo" },
                    { "RoleName", "Chức vụ" },
                    { "HourlyWage", "Lương giờ" }
                };
            }
            return null;
        }

        // =============================================================
        // 1. EXPORT (GIỮ NGUYÊN LOGIC, CHỈ GỌI HÀM GET MAP)
        // =============================================================
        public static bool ExportList<T>(string filePath, List<T> data, string sheetName)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    var properties = typeof(T).GetProperties().ToList();

                    // 1. Lấy Map từ hàm chung
                    Dictionary<string, string> headerMap = GetHeaderMap<T>();

                    List<PropertyInfo> orderedProperties;
                    if (headerMap != null)
                    {
                        var desiredOrder = headerMap.Keys.ToList();
                        orderedProperties = new List<PropertyInfo>();
                        foreach (var name in desiredOrder)
                        {
                            var p = properties.FirstOrDefault(x => x.Name == name);
                            if (p != null) orderedProperties.Add(p);
                        }
                    }
                    else
                    {
                        orderedProperties = properties;
                    }

                    // --- HEADER ---
                    for (int i = 0; i < orderedProperties.Count; i++)
                    {
                        string headerName = orderedProperties[i].Name;
                        // Map: Property -> Tiếng Việt
                        if (headerMap != null && headerMap.ContainsKey(headerName))
                        {
                            headerName = headerMap[headerName];
                        }
                        worksheet.Cell(1, i + 1).Value = headerName;
                    }

                    // Format Header
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
                    headerRow.Style.Font.FontColor = XLColor.White;

                    // --- DATA ---
                    for (int r = 0; r < data.Count; r++)
                    {
                        worksheet.Cell(r + 2, 1).Value = r + 1; // STT
                        for (int c = 1; c < orderedProperties.Count; c++)
                        {
                            var value = orderedProperties[c].GetValue(data[r]);
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
        // 2. IMPORT (SỬA LẠI ĐỂ DÙNG REVERSE MAP)
        // =============================================================
        public static List<T> ImportList<T>(string filePath) where T : new()
        {
            var list = new List<T>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var headerRow = worksheet.Row(1);
                var properties = typeof(T).GetProperties();

                // 1. Lấy Map gốc (Property -> Tiếng Việt)
                var headerMap = GetHeaderMap<T>();

                // 2. Tạo Map Ngược (Tiếng Việt -> Property)
                // Ví dụ: "Tên nguyên liệu" -> "IngName"
                var reverseMap = new Dictionary<string, string>();
                if (headerMap != null)
                {
                    foreach (var kvp in headerMap)
                    {
                        // Key mới là Tiếng Việt, Value mới là Property Name
                        if (!reverseMap.ContainsKey(kvp.Value))
                            reverseMap.Add(kvp.Value, kvp.Key);
                    }
                }

                // 3. Map cột Excel sang Property Name
                // Key: Tên Property (IngName), Value: Cột số mấy (2)
                var columnMap = new Dictionary<string, int>();

                foreach (var cell in headerRow.CellsUsed())
                {
                    string headerText = cell.GetString().Trim(); // Lấy tên cột trong Excel ("Tên nguyên liệu")
                    string propertyName = headerText; // Mặc định giả sử tên cột = tên Property

                    // Nếu có trong map ngược, thì đổi tên Tiếng Việt về tên Property thật
                    if (reverseMap.ContainsKey(headerText))
                    {
                        propertyName = reverseMap[headerText];
                    }

                    columnMap[propertyName] = cell.Address.ColumnNumber;
                }

                // 4. Đọc dữ liệu (Giữ nguyên logic cũ, chỉ thay đổi cách lấy columnMap ở trên)
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    T item = new T();
                    foreach (var prop in properties)
                    {
                        // Lúc này columnMap đã chứa Key là tên Property thật (IngName)
                        if (columnMap.ContainsKey(prop.Name))
                        {
                            int colIndex = columnMap[prop.Name];
                            var cellValue = row.Cell(colIndex).GetValue<string>();

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                try
                                {
                                    object safeValue = ChangeTypeSafe(cellValue, prop.PropertyType);
                                    prop.SetValue(item, safeValue);
                                }
                                catch { }
                            }
                        }
                    }
                    list.Add(item);
                }
            }
            return list;
        }

        private static object ChangeTypeSafe(string value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value)) return null;
                conversionType = Nullable.GetUnderlyingType(conversionType);
            }
            if (conversionType == typeof(decimal))
            {
                return Convert.ToDecimal(double.Parse(value));
            }
            // Thêm xử lý ngày tháng nếu cần
            if (conversionType == typeof(DateTime))
            {
                return DateTime.Parse(value);
            }
            return Convert.ChangeType(value, conversionType);
        }
    }
}