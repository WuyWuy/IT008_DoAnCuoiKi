using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using Microsoft.Win32;
using QuanLyCaPhe.Views.Components; // <-- Import Custom MessageBox

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
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MsgType.Error);
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
                JetMoonMessageBox.Show("Vui lòng chọn món cần sửa!", "Chưa chọn dòng", MsgType.Warning);
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

            var result = JetMoonMessageBox.Show(
                $"Bạn có chắc muốn xóa món '{selectedProduct.ProName}' không?",
                "Xác nhận xóa",
                MsgType.Question,
                true); // Hiện nút Cancel

            if (result == true)
            {
                if (ProductDAO.Instance.DeleteProduct(selectedProduct.Id))
                {
                    JetMoonMessageBox.Show("Đã xóa thành công!", "Hoàn tất", MsgType.Success);
                    LoadData();
                }
                else
                {
                    JetMoonMessageBox.Show(
                        "Không thể xóa món này (Có thể món đã từng được bán trong hóa đơn).",
                        "Lỗi ràng buộc",
                        MsgType.Error);
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
                    JetMoonMessageBox.Show($"Xử lý xong!\n- Thêm mới: {countAdd} món\n- Cập nhật giá: {countUpdate} món", "Kết quả", MsgType.Success);
                }
                catch (Exception ex)
                {
                    JetMoonMessageBox.Show("Lỗi: " + ex.Message, "Lỗi Import", MsgType.Error);
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
                JetMoonMessageBox.Show("Xuất xong!", "Thông báo", MsgType.Success);
            }
        }

        private void BtnManageRecipe_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedProduct = menuItem?.DataContext as Product;

            if (selectedProduct == null)
            {
                JetMoonMessageBox.Show("Vui lòng chọn món cần quản lý công thức!", "Chưa chọn dòng", MsgType.Warning);
                return;
            }

            var w = new RecipeEditorWindow(selectedProduct.Id, selectedProduct.ProName);
            w.ShowDialog();
        }
    }
}