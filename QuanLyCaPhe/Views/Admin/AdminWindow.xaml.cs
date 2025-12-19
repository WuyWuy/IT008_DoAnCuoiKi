using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation; // Cần dòng này cho Animation
using QuanLyCaPhe.Views.Components;
using QuanLyCaPhe.Views.Login;
using QuanLyCaPhe.Views.SharedPage;

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
        }

        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }

        // ... (Các hàm điều hướng giữ nguyên) ...
        private void Homebtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new HomePage()); }
        private void Prodbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new ProductsPage()); }
        private void Ingredbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new IngredientPage()); }
        private void Staffbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new UserPage()); }
        private void Suplybtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new InputInfoPage()); }
        private void Billbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new BillsPage()); }
        private void Timelbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new SchedulePage()); }
        private void Profitbtn_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(new ProfitPage()); }

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

            if (! _isSidebarOpen)
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