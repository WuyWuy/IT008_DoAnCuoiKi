using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System.Windows;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class ProductDetailWindow : Window
    {
        private Product _editingProduct = null;

        public ProductDetailWindow(Product productToEdit = null)
        {
            InitializeComponent();
            _editingProduct = productToEdit;

            if (_editingProduct != null) // Chế độ Sửa
            {
                txtName.Text = _editingProduct.ProName;
                // Format tiền tệ cho đẹp, bỏ các ký tự không phải số khi lưu
                txtPrice.Text = _editingProduct.Price.ToString("G29");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên món!");
                return;
            }

            string name = txtName.Text;
            decimal price = 0;

            if (!decimal.TryParse(txtPrice.Text, out price))
            {
                MessageBox.Show("Giá tiền phải là số!");
                return;
            }

            // --- BẮT ĐẦU KHỐI TRY-CATCH AN TOÀN ---
            try
            {
                bool result = false;

                if (_editingProduct == null) // Thêm mới
                {
                    result = ProductDAO.Instance.InsertProduct(name, price);
                }
                else // Sửa
                {
                    result = ProductDAO.Instance.UpdateProduct(_editingProduct.Id, name, price);
                }

                // Nếu chạy đến đây nghĩa là không bị lỗi sập app
                if (result)
                {
                    MessageBox.Show("Lưu thành công!", "Thông báo");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Không thể lưu (Có thể do dữ liệu không hợp lệ).", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                // --- ĐÂY LÀ CHỖ QUAN TRỌNG NHẤT ---
                // Thay vì crash, nó sẽ hiện thông báo lỗi chi tiết ra cho bạn đọc
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Crash Report", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}