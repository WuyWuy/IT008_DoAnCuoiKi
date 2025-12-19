using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class ActivitiesPage : Page
    {
        public ObservableCollection<Activity> Activities { get; set; }

        public ActivitiesPage()
        {
            InitializeComponent();
            Activities = new ObservableCollection<Activity>();
            lbActivities.ItemsSource = Activities;

            // 1. Load dữ liệu (Đã bọc try-catch để không bao giờ crash)
            LoadHistoryAsync();

            // 2. Quản lý sự kiện: Đăng ký khi Load, Hủy khi Unload
            this.Loaded += (s, e) =>
            {
                GlobalService.OnActivityOccurred -= OnNewActivity;
                GlobalService.OnActivityOccurred += OnNewActivity;
            };

            this.Unloaded += (s, e) =>
            {
                GlobalService.OnActivityOccurred -= OnNewActivity;
            };
        }

        private void OnNewActivity(Activity act)
        {
            Dispatcher.Invoke(() =>
            {
                Activities.Insert(0, act);
                if (Activities.Count > 50) Activities.RemoveAt(Activities.Count - 1);
            });
        }

        // --- HÀM SỬA LỖI CRASH ---
        private async void LoadHistoryAsync()
        {
            try
            {
                // Chạy ở background để không đơ giao diện
                var list = await Task.Run(() => ActivityDAO.Instance.GetRecentActivities(20));

                foreach (var item in list)
                {
                    Activities.Add(item);
                }
            }
            catch (Exception ex)
            {
                // NẾU LỖI: Không crash, chỉ thêm dòng thông báo đỏ vào list
                Activities.Add(new Activity
                {
                    Description = "Lỗi kết nối",
                    Detail = "Kiểm tra lại Connection String hoặc Bảng DB. Lỗi: " + ex.Message,
                    CreatedDate = DateTime.Now,
                    IconColor = "#EF4444", // Đỏ
                    IconPath = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"
                });
            }
        }
    }
}