using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System;
using System.Collections.Generic;

namespace QuanLyCaPhe.Services
{
    public static class GlobalService
    {
        // 1. CÁC SỰ KIỆN (EVENTS) ĐỂ UI LẮNG NGHE
        // Sự kiện khi có hoạt động mới (Order/Thanh toán) -> Bắn ra 1 object Activity
        public static event Action<Activity> OnActivityOccurred;

        // Sự kiện khi danh sách cảnh báo thay đổi -> UI cần cập nhật lại ListBox
        public static event Action OnWarningUpdated;

        // 2. DỮ LIỆU CẢNH BÁO (Lưu trong RAM để truy xuất nhanh)
        public static List<string> CurrentWarnings { get; private set; } = new List<string>();

        // 3. HÀM GHI NHẬN HOẠT ĐỘNG (Dùng cho StaffWindow gọi)
        public static void RecordActivity(string type, string desc, string detail)
        {
            // A. Lưu vào Cơ sở dữ liệu (Để tắt máy mở lại vẫn còn)
            // Lưu ý: Trong DAO đã code sẵn việc lấy DateTime.Now truyền xuống SQL
            ActivityDAO.Instance.InsertActivity(type, desc, detail);

            // B. Tạo object Activity để bắn lên giao diện NGAY LẬP TỨC (Realtime)
            // Không cần query lại DB để lấy ID, chỉ cần hiển thị cho đẹp
            var act = new Activity
            {
                ActivityType = type,
                Description = desc,
                Detail = detail,
                CreatedDate = DateTime.Now // Quan trọng: Khớp với giờ hệ thống
            };

            // C. Cấu hình Icon/Màu sắc ngay tại đây để UI hiển thị đẹp luôn
            switch (type)
            {
                case "Payment": // Thanh toán: Màu xanh lá
                    act.IconColor = "#10B981";
                    act.IconPath = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 17l-5-5 1.41-1.41L10 16.17l7.59-7.59L19 10z";
                    break;

                case "Order": // Order: Màu xanh dương
                    act.IconColor = "#3B82F6";
                    act.IconPath = "M20 2H4v20h16V2zm-4 12h-4v4h-2v-4H8v-2h4V8h2v4h2v2z";
                    break;

                default: // Khác: Màu xám
                    act.IconColor = "#64748B";
                    act.IconPath = "M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z";
                    break;
            }

            // D. Kích hoạt sự kiện -> LastBillPage sẽ nhận được act này và thêm vào đầu danh sách
            OnActivityOccurred?.Invoke(act);

            // E. Tiện thể kiểm tra luôn xem có gì cần cảnh báo không (Ví dụ vừa bán xong thì hết hàng)
            CheckWarnings();
        }

        // 4. HÀM KIỂM TRA CẢNH BÁO (Hết hàng, Lỗi...)
        public static void CheckWarnings()
        {
            CurrentWarnings.Clear();

            // Logic: Kiểm tra kho nguyên liệu
            try
            {
                var ingredients = IngredientDAO.Instance.GetListIngredient();
                foreach (var ing in ingredients)
                {
                    // Ngưỡng cảnh báo: Dưới 10 đơn vị
                    if (ing.Quantity < 10)
                    {
                        CurrentWarnings.Add($"⚠️ {ing.IngName} sắp hết (Còn {ing.Quantity} {ing.Unit})");
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi kết nối DB nếu có để không làm crash app
            }

            // Bắn tín hiệu để WarningsPage cập nhật lại giao diện
            OnWarningUpdated?.Invoke();
        }
    }
}