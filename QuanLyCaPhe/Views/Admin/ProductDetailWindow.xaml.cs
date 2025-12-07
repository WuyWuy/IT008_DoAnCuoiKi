using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
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
    /// Interaction logic for ProductDetailWindow.xaml
    /// </summary>
    public partial class ProductDetailWindow : Window
    {
        // Biến để lưu món đang sửa (nếu có)
        private Products _editingProduct = null;

        // Constructor nhận tham số (Nếu null là Thêm, có dữ liệu là Sửa)
        public ProductDetailWindow(Products productToEdit = null)
        {
            InitializeComponent();
            _editingProduct = productToEdit;

            // Nếu là chế độ Sửa -> Đổ dữ liệu cũ lên ô nhập
            if (_editingProduct != null)
            {
                txtName.Text = _editingProduct.ProName;
                txtPrice.Text = _editingProduct.Price.ToString("N0");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (string.IsNullOrEmpty(txtName.Text)) return;

            // 2. Lưu vào Database
            string sql = "";

            if (_editingProduct == null) // CHẾ ĐỘ THÊM
            {
                sql = $"INSERT INTO Products (ProName, Price) VALUES (N'{txtName.Text}', {txtPrice.Text})";
            }
            else // CHẾ ĐỘ SỬA
            {
                sql = $"UPDATE Products SET ProName = N'{txtName.Text}', Price = {txtPrice.Text} WHERE Id = {_editingProduct.Id}";
            }

            DBHelper.ExecuteNonQuery(sql);

            // 3. Đóng cửa sổ và báo thành công
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
