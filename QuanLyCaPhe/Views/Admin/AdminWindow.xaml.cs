using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
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
            MainFrame.Navigate(new IngredientsPage());
        }

        private void Staffbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StaffPage());
        }

        private void Timelbtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TimeLinePage());
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
    }
}
