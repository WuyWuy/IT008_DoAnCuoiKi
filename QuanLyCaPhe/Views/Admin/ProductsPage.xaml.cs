using QuanLyCaPhe.Models;
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

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        public ObservableCollection<Products> ProductsList { get; set; }
        public ProductsPage()
        {
            InitializeComponent();


            // Khởi tạo collection
            ProductsList = new ObservableCollection<Products>();

            // Gán ItemsSource cho DataGrid
            Productsdg.ItemsSource = ProductsList;

            // Thêm dữ liệu demo
            LoadDemoData();
        }

        private void LoadDemoData()
        {
            // Thêm 10 sản phẩm demo
            ProductsList.Add(new Products { Id = 1, ProName = "Cà phê đen đá", Price = 25000 });
            ProductsList.Add(new Products { Id = 2, ProName = "Cà phê sữa đá", Price = 30000 });
            ProductsList.Add(new Products { Id = 3, ProName = "Cà phê bạc xỉu", Price = 35000 });
            ProductsList.Add(new Products { Id = 4, ProName = "Trà đào cam sả", Price = 40000 });
            ProductsList.Add(new Products { Id = 5, ProName = "Trà vải", Price = 38000 });
            ProductsList.Add(new Products { Id = 6, ProName = "Sinh tố bơ", Price = 45000 });
            ProductsList.Add(new Products { Id = 7, ProName = "Sinh tố xoài", Price = 42000 });
            ProductsList.Add(new Products { Id = 8, ProName = "Nước ép cam", Price = 35000 });
            ProductsList.Add(new Products { Id = 9, ProName = "Nước ép dưa hấu", Price = 32000 });
            ProductsList.Add(new Products { Id = 10, ProName = "Bánh mì chảo", Price = 55000 });
            ProductsList.Add(new Products { Id = 11, ProName = "Bánh croissant", Price = 28000 });
            ProductsList.Add(new Products { Id = 12, ProName = "Mì Ý sốt bò bằm", Price = 65000 });
            ProductsList.Add(new Products { Id = 13, ProName = "Kem matcha", Price = 38000 });
            ProductsList.Add(new Products { Id = 14, ProName = "Kem chocolate", Price = 38000 });
            ProductsList.Add(new Products { Id = 15, ProName = "Bánh tiramisu", Price = 45000 });
        }
    }
}
