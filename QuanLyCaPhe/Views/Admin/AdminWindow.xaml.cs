using DocumentFormat.OpenXml.Drawing.Diagrams;
using QuanLyCaPhe.Views.Login;
using QuanLyCaPhe.Views.SharedPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        // Flag to indicate the user requested logout (so Closing handler won't shut down the app)
        private bool _isLoggingOut = false;

        public AdminWindow()
        {
            InitializeComponent();
            this.Loaded += AdminWindow_Loaded;
            this.Closing += AdminWindow_Closing;
            //curClick = Homebtn;
        }

        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }

        private void Homebtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }

        private void Prodbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductsPage());
        }

        private void Ingredbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IngredientPage());
        }

        private void Staffbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UserPage());
        }

        private void Timelbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SchedulePage());
        }

        private void Profitbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfitPage());
        }

        private void Sidebar_MouseEnter(object sender, MouseEventArgs e)
        {
            //Sidebar.Visibility = Visibility.Collapsed;
        }

        private void Sidebar_MouseLeave(object sender, MouseEventArgs e)
        {
            //Sidebar.Visibility = Visibility.Visible;
        }

        private void Suplybtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InputInfoPage());
        }

        private void Billbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BillsPage());
        }

        private void Menubtn_Click(object sender, RoutedEventArgs e)
        {
            double targetWidth = Sidebar.ActualWidth > 10 ? 0 : 250;

            // Tạo Animation thay đổi chiều rộng
            DoubleAnimation widthAnimation = new DoubleAnimation();
            widthAnimation.To = targetWidth;
            widthAnimation.Duration = TimeSpan.FromMilliseconds(300); 

            // Thêm hiệu ứng gia tốc (Easing) cho mượt (Gia tốc đầu và cuối)
            widthAnimation.EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut, Power = 3 };

            // Bắt đầu chạy
            Sidebar.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);

            // Fade
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = targetWidth > 0 ? 1 : 0; 
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(200);

            Sidebar.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        private void AdminWindow_Closing(object? sender, CancelEventArgs e)
        {
            // stop timers / cleanup if needed...
            // If user is logging out, do not shut down the application; simply allow close so LoginWindow can show.
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
            // Mark that we're logging out so Closing handler won't call Shutdown
            _isLoggingOut = true;

            // Return to login window: open a new LoginWindow and close this admin window
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
