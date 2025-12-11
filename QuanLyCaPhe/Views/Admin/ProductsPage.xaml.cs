using QuanLyCaPhe.DataAccess;
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

        // Sự kiện Click MENU SỬA
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy MenuItem (sender) và truy cập DataContext để có đối tượng Products
            var menuItem = sender as MenuItem;
            var selectedProduct = menuItem?.DataContext as Products; // Sửa lỗi ở đây!

            if (selectedProduct == null)
            {
                // Điều này có thể xảy ra nếu ContextMenu không được click trên một Row hợp lệ
                MessageBox.Show("Vui lòng chọn món cần sửa!");
                return;
            }

            // Đảm bảo hàng được chọn trong DataGrid (chỉ để highlight)
            Productsdg.SelectedItem = selectedProduct;

            // Mở cửa sổ sửa (Truyền món đó vào)
            // Giả định ProductDetailWindow là tên cửa sổ của bạn
            var f = new ProductDetailWindow(selectedProduct);

            // Khi cửa sổ ProductDetailWindow đóng với kết quả OK (true), chúng ta cần tải lại dữ liệu.
            // LƯU Ý: Nếu bạn chỉ sửa dữ liệu trong đối tượng selectedProduct và không cần tải lại toàn bộ DB,
            // bạn không cần gọi LoadDemoData() vì ObservableCollection sẽ tự cập nhật.
            if (f.ShowDialog() == true)
            {
                // Nếu cửa sổ ProductDetailWindow có logic cập nhật trực tiếp vào DB,
                // và bạn muốn refresh DataGrid:
                LoadDemoData(); // Nếu bạn muốn refresh lại hoàn toàn từ DB, hãy gọi hàm tải dữ liệu thực tế ở đây.
                // Nếu không có logic cập nhật, bạn có thể chỉ cần làm mới giao diện (nếu cần)
            }
        }

        // Sự kiện Click MENU XÓA
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy MenuItem (sender) và truy cập DataContext để có đối tượng Products
            var menuItem = sender as MenuItem;
            var selectedProduct = menuItem?.DataContext as Products; // Sửa lỗi ở đây!

            if (selectedProduct == null) return;

            // Hỏi cho chắc
            var result = MessageBox.Show($"Bạn có chắc muốn xóa món '{selectedProduct.ProName}' không?",
                                         "Xác nhận xóa",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 1. Xóa trong Database
                    // CHÚ Ý: Cần kiểm tra kỹ việc sử dụng String Interpolation trực tiếp trong SQL (SQL Injection)
                    string sql = $"DELETE FROM Products WHERE Id = {selectedProduct.Id}";
                    DBHelper.ExecuteNonQuery(sql);

                    // 2. Xóa trên giao diện (ObservableCollection tự động cập nhật DataGrid)
                    ProductsList.Remove(selectedProduct);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Xóa thất bại: {ex.Message}", "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy MenuItem (sender) và truy cập DataContext để có đối tượng Products
            var menuItem = sender as MenuItem;
            var selectedProduct = menuItem?.DataContext as Products; // Sửa lỗi ở đây!

            // Mở cửa sổ sửa (Truyền món đó vào)
            // Giả định ProductDetailWindow là tên cửa sổ của bạn
            var f = new ProductDetailWindow();

            // Khi cửa sổ ProductDetailWindow đóng với kết quả OK (true), chúng ta cần tải lại dữ liệu.
            // LƯU Ý: Nếu bạn chỉ sửa dữ liệu trong đối tượng selectedProduct và không cần tải lại toàn bộ DB,
            // bạn không cần gọi LoadDemoData() vì ObservableCollection sẽ tự cập nhật.
            if (f.ShowDialog() == true)
            {
                // Nếu cửa sổ ProductDetailWindow có logic cập nhật trực tiếp vào DB,
                // và bạn muốn refresh DataGrid:
                LoadDemoData(); // Nếu bạn muốn refresh lại hoàn toàn từ DB, hãy gọi hàm tải dữ liệu thực tế ở đây.
                // Nếu không có logic cập nhật, bạn có thể chỉ cần làm mới giao diện (nếu cần)
            }
        }
    }
}
