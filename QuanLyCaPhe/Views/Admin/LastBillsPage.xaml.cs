using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Staff;

namespace QuanLyCaPhe.Views.Admin
{
    public enum ActivityType { Order, Payment, Warning }

    // =========================================================
    // 2. LOGIC CHÍNH CỦA CỬA SỔ
    // =========================================================
    public partial class LastBillsPage : Page
    {
        // Danh sách liên kết với giao diện (ListBox)
        public ObservableCollection<Activity> RecentActivities { get; set; }

        public LastBillsPage()
        {
            InitializeComponent();

            // 1. Khởi tạo danh sách
            RecentActivities = new ObservableCollection<Activity>();

            // 2. Gán vào ListBox (Đảm bảo bên XAML ListBox có tên x:Name="lbRecentActivities")
            lbRecentActivities.ItemsSource = RecentActivities;

            // 3. Nạp dữ liệu mẫu (Demo Data)
            LoadDemoData();
        }

        /// <summary>
        /// Hàm tạo dữ liệu giả để giao diện không bị trống lúc đầu
        /// </summary>
        private void LoadDemoData()
        {
            // Thêm 4 giao dịch mẫu vào danh sách
            AddTransaction(ActivityType.Order, "Bàn 02", "2 Cà phê đen, 1 Bạc xỉu", "30 phút trước");
            AddTransaction(ActivityType.Payment, "Bàn 05", "Tổng: 75.000 VNĐ", "10 phút trước");
            AddTransaction(ActivityType.Order, "Bàn 10", "1 Trà đào cam sả", "5 phút trước");
            AddTransaction(ActivityType.Payment, "Bàn 01", "Tổng: 45.000 VNĐ", "1 phút trước");
        }

        /// <summary>
        /// Hàm thêm một giao dịch mới vào đầu danh sách (Dùng khi bấm nút)
        /// </summary>
        public void AddTransaction(ActivityType type, string title, string detail, string time = "Vừa xong")
        {
            // Cấu hình Icon và Màu sắc dựa trên loại hoạt động
            string iconPath = "";
            string iconColor = "";
            string desc = "";

            switch (type)
            {
                case ActivityType.Payment:
                    // Icon Check (Thanh toán thành công) - Màu Xanh Lá
                    iconPath = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 17l-5-5 1.41-1.41L10 16.17l7.59-7.59L19 10z";
                    iconColor = "#10B981";
                    desc = $"Thanh toán: {title}";
                    break;

                case ActivityType.Order:
                    // Icon Dấu Cộng (Order mới) - Màu Xanh Dương
                    iconPath = "M20 2H4v20h16V2zm-4 12h-4v4h-2v-4H8v-2h4V8h2v4h2v2z";
                    iconColor = "#54A0FF";
                    desc = $"Order mới: {title}";
                    break;

                case ActivityType.Warning:
                    // Icon Tam giác (Cảnh báo) - Màu Cam
                    iconPath = "M12 2L1 21h22M12 6l.01 10-2-2z M13 18h-2v-2h2v2z";
                    iconColor = "#F59E0B";
                    desc = $"Cảnh báo: {title}";
                    break;
            }

            // Tạo đối tượng mới
            var newActivity = new Activity
            {
                Description = desc,
                Detail = detail,
                TimeAgo = time,
                IconPath = iconPath,
                IconColor = iconColor
            };

            // Chèn vào ĐẦU danh sách (vị trí 0) để nó hiện lên trên cùng
            RecentActivities.Insert(0, newActivity);

            // Giữ danh sách gọn gàng (chỉ hiện 5 cái mới nhất)
            if (RecentActivities.Count > 5)
            {
                RecentActivities.RemoveAt(RecentActivities.Count - 1);
            }
        }
    }
}
