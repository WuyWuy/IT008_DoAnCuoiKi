using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation; // Cần dòng này cho Animation
using QuanLyCaPhe.Views.Components;
using QuanLyCaPhe.Views.Login;
using QuanLyCaPhe.Views.SharedPage;
using QuanLyCaPhe.Services;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class AdminWindow : Window
    {
        private bool _isLoggingOut = false;

        // [FIX] Thêm biến cờ để theo dõi trạng thái Sidebar (Mặc định là đang mở - true)
        private bool _isSidebarOpen = true;

        public AdminWindow()
        {
            InitializeComponent();
            this.Loaded += AdminWindow_Loaded;
            this.Closing += AdminWindow_Closing;

            // Subscribe to global activity events so the Admin window can refresh overview when staff deleted
            GlobalService.OnActivityOccurred += HandleGlobalActivity;
        }

        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }

        private void HandleGlobalActivity(QuanLyCaPhe.Models.Activity act)
        {
            // Handle various activities: staff deletion (existing), and product-related activities (added/updated/deleted)
            try
            {
                if (act == null) return;

                // Run UI updates on dispatcher
                Dispatcher.Invoke(() =>
                {
                    // Existing logic: when staff deleted, refresh HomePage overview
                    if (act.ActivityType == "Delete" && act.Description?.Contains("nhân viên") == true)
                    {
                        if (MainFrame.Content is HomePage home)
                        {
                            home.RefreshOverview();
                        }
                    }

                    // New: product activities -> refresh overview and products list when required
                    // We accept either a dedicated ActivityType "Product" or any activity whose description mentions "món" (case-insensitive)
                    bool isProductActivity = string.Equals(act.ActivityType, "Product", StringComparison.OrdinalIgnoreCase)
                                             || (act.ActivityType != null && act.ActivityType.IndexOf("món", StringComparison.OrdinalIgnoreCase) >= 0)
                                             || (act.Description != null && act.Description.IndexOf("món", StringComparison.OrdinalIgnoreCase) >= 0);

                    if (isProductActivity)
                    {
                        // Refresh HomePage counters (product count etc.)
                        if (MainFrame.Content is HomePage hp)
                        {
                            hp.RefreshOverview();
                        }

                        // If admin currently viewing ProductsPage, refresh it by navigating to a new instance
                        // (This keeps the UI in sync after imports / edits / deletes)
                        if (MainFrame.Content is ProductsPage)
                        {
                            MainFrame.Navigate(new ProductsPage());
                        }
                    }
                });
            }
            catch
            {
                // Swallow exceptions from handling events to avoid app crash
            }
        }

        private void Homebtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new HomePage()); }
        private void Prodbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new ProductsPage()); }
        private void Ingredbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new IngredientPage()); }
        private void Staffbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new UserPage()); }
        private void Suplybtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new InputInfoPage()); }
        private void Billbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new BillsPage()); }
        private void Timelbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new SchedulePage()); }
        private void Profitbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new ProfitPage()); }
        private void Tablebtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new TablePage()); }
        private void Paymentbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new PaymentPage()); }

        // [FIX] SỬA LẠI HÀM NÀY
        private void Menubtn_Click(object sender, RoutedEventArgs e)
        {
            // 1. Đảo ngược trạng thái
            _isSidebarOpen = !_isSidebarOpen;

            // 2. Xác định chiều rộng mục tiêu dựa trên biến cờ
            double targetWidth = _isSidebarOpen ? 250 : 0;
            double targetOpacity = _isSidebarOpen ? 1 : 0;

            // 3. Animation chiều rộng (Width)
            DoubleAnimation widthAnimation = new DoubleAnimation();
            widthAnimation.To = targetWidth;
            widthAnimation.Duration = TimeSpan.FromMilliseconds(300); // Tốc độ mượt mà
            widthAnimation.EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut, Power = 3 }; // Hiệu ứng gia tốc

            // 4. Animation độ mờ (Opacity) - Fade in/out
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = targetOpacity;
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(200);

            // 5. Bắt đầu chạy hiệu ứng
            Sidebar.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
            Sidebar.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);

            if (!_isSidebarOpen)
                Logoutbtn.HorizontalAlignment = HorizontalAlignment.Center;
            else
                Logoutbtn.HorizontalAlignment = HorizontalAlignment.Right;
        }

        private void AdminWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_isLoggingOut)
            {
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }

        private void Logoutbtn_Click(object sender, RoutedEventArgs e)
        {
            var res = JetMoonMessageBox.Show(
                    $"Bạn có chắc chắn muốn đăng xuất không ?",
                    "Xác nhận đắng xuất",
                    MsgType.Question,
                    true);

            if (res == false) return;

            _isLoggingOut = true;
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

    }
}