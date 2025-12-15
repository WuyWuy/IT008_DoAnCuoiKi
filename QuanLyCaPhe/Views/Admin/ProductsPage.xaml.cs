using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using Microsoft.Win32;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class ProductsPage : Page
    {
        // --- Khởi tạo & load ---
        public List<Product> ProductsList { get; set; }

        public ProductsPage()
        {
            InitializeComponent();
            Loaded += ProductsPage_Loaded;
        }

        private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                ProductsList = ProductDAO.Instance.GetListProduct();
                Productsdg.ItemsSource = ProductsList;
            }
            catch
            {
                MessageBox.Show("mày đây r con chó");
            }
        }

        // --- Đếm STT --- 
        private void Productsdg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        // --- Tìm kiếm ---
        private void SearchBar_Clicked(object sender, string e)
        {
            Productsdg.ItemsSource = ProductDAO.Instance.SearchProductByName(SearchBar.Text);
        }

        // --- Thêm Mới ---
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var f = new ProductDetailWindow();
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Sửa Món ---
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedProduct = menuItem?.DataContext as Product;

            if (selectedProduct == null)
            {
                MessageBox.Show("Vui lòng chọn món cần sửa!");
                return;
            }

            var f = new ProductDetailWindow(selectedProduct);
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Xóa Món ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedProduct = menuItem?.DataContext as Product;

            if (selectedProduct == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa món '{selectedProduct.ProName}' không?",
                                         "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (ProductDAO.Instance.DeleteProduct(selectedProduct.Id))
                {
                    MessageBox.Show("Đã xóa thành công!");
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Không thể xóa món này (Có thể món đã từng được bán trong hóa đơn).",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }

        // --- Click nút Import/Export Excel ---
        private void IETable_Clicked(object sender, string e)
        {
            if (e == "Import") ImportData();
            else ExportData();
        }
        // --- Import ---
        private void ImportData()
        {
            var ofd = new OpenFileDialog() { Filter = "Excel Files|*.xlsx" };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    List<Product> importedList = ExcelHelper.ImportList<Product>(ofd.FileName);

                    int countAdd = 0;
                    int countUpdate = 0;

                    foreach (var item in importedList)
                    {
                        string excelName = item.ProName.Trim();
                        int existingId = ProductDAO.Instance.GetIdByName(excelName);

                        if (existingId != -1)
                        {
                            if (ProductDAO.Instance.UpdateProduct(existingId, excelName, item.Price))
                                countUpdate++;
                        }
                        else
                        {
                            if (ProductDAO.Instance.InsertProduct(excelName, item.Price))
                                countAdd++;
                        }
                    }

                    LoadData();
                    MessageBox.Show($"Xử lý xong!\n- Thêm mới: {countAdd} món\n- Cập nhật giá: {countUpdate} món", "Kết quả");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
        // --- Export ---
        private void ExportData()
        {
            var sfd = new SaveFileDialog() { Filter = "Excel|*.xlsx", FileName = "DS_MonAn.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                var list = ProductDAO.Instance.GetListProduct();
                ExcelHelper.ExportList<Product>(sfd.FileName, list, "SanPhams");
                MessageBox.Show("Xuất xong!");
            }
        }
    }
}