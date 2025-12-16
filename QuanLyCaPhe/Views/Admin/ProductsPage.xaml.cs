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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();
        }

        private void BtnManageRecipe_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedProduct = menuItem?.DataContext as Product;

            if (selectedProduct == null)
            {
                MessageBox.Show("Vui lòng chọn món cần quản lý công thức!");
                return;
            }

            var w = new RecipeEditorWindow(selectedProduct.Id, selectedProduct.ProName);
            w.ShowDialog();
            // no direct change to products; recipes managed separately
        }
    }
}
