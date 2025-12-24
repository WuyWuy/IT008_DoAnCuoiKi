using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Services; // Import Service để lắng nghe sự kiện

namespace QuanLyCaPhe.Views.Admin
{
    public partial class TopProductPage : Page
    {
        // Sử dụng ObservableCollection để Binding dữ liệu realtime
        public ObservableCollection<TopProduct> TopProducts { get; set; }

        public TopProductPage()
        {
            InitializeComponent();

            // 1. Khởi tạo Collection
            TopProducts = new ObservableCollection<TopProduct>();
            icBestSellers.ItemsSource = TopProducts;

            // 2. Load dữ liệu lần đầu (Chạy bất đồng bộ)
            LoadDataAsync();

            // 3. Đăng ký sự kiện: Khi có hoạt động mới (ví dụ Order/Thanh toán) -> Load lại Top
            this.Loaded += (s, e) =>
            {
                GlobalService.OnActivityOccurred -= OnActivityOccurred; // Đảm bảo không đăng ký trùng
                GlobalService.OnActivityOccurred += OnActivityOccurred;
            };

            this.Unloaded += (s, e) =>
            {
                GlobalService.OnActivityOccurred -= OnActivityOccurred;
            };
        }

        // Sự kiện khi có hoạt động mới xảy ra trong hệ thống
        private void OnActivityOccurred(Activity act)
        {
            // Chỉ cần load lại nếu hoạt động liên quan đến bán hàng (Order hoặc Thanh toán)
            // Nếu bạn không chắc chắn description ghi gì, cứ load lại hết cũng được (vì query Top 3 khá nhẹ)
            Dispatcher.Invoke(() =>
            {
                LoadDataAsync();
            });
        }

        // Hàm load dữ liệu chạy ngầm (giống ActivitiesPage)
        public async void LoadDataAsync()
        {
            try
            {
                // Chạy query DB ở luồng phụ để không đơ UI
                var list = await Task.Run(() => TopProductDAO.Instance.GetTop3BestSellers());

                // Cập nhật giao diện ở luồng chính
                Dispatcher.Invoke(() =>
                {
                    TopProducts.Clear(); // Xóa danh sách cũ

                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            TopProducts.Add(item);
                        }
                        txtNoData.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtNoData.Visibility = Visibility.Visible;
                    }
                });
            }
            catch (Exception)
            {
                // Xử lý lỗi nhẹ nhàng (ví dụ hiện text không có dữ liệu)
                Dispatcher.Invoke(() => txtNoData.Visibility = Visibility.Visible);
            }
        }
    }
}