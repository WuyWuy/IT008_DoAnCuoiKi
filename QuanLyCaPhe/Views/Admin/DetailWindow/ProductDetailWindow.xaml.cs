using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import Custom MessageBox
using System; // Thêm System để dùng Exception
using System.Collections.Generic;
using System.Windows;
using QuanLyCaPhe.Services; // <-- Added to record activities

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
                txtPrice.Text = _editingProduct.Price.ToString("G29");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                JetMoonMessageBox.Show("Vui lòng nhập tên món!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            string name = txtName.Text;
            decimal price = 0;

            if (!decimal.TryParse(txtPrice.Text, out price))
            {
                JetMoonMessageBox.Show("Giá tiền phải là số hợp lệ!", "Lỗi nhập liệu", MsgType.Error);
                return;
            }

            try
            {
                bool result = false;

                if (_editingProduct == null)
                {
                    result = ProductDAO.Instance.InsertProduct(name, price);
                }
                else
                {
                    result = ProductDAO.Instance.UpdateProduct(_editingProduct.Id, name, price);
                }

                if (result)
                {
                    // Record product create/update activity so ActivitiesPage shows it
                    try
                    {
                        if (_editingProduct == null)
                        {
                            GlobalService.RecordActivity(
                                "Create",
                                "Thêm món",
                                $"Đã thêm món '{name}' với giá {price:N0} đ");
                        }
                        else
                        {
                            string oldName = _editingProduct.ProName ?? string.Empty;
                            decimal oldPrice = _editingProduct.Price;

                            // Build a message that only includes changed fields
                            var changes = new List<string>();

                            if (!string.Equals(oldName?.Trim(), name?.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                changes.Add($"Tên: '{oldName}' -> '{name}'");
                            }

                            if (oldPrice != price)
                            {
                                changes.Add($"Giá: {oldPrice:N0} đ -> {price:N0} đ");
                            }

                            if (changes.Count > 0)
                            {
                                // Use the oldName as the item label if available, otherwise the new name
                                string itemLabel = !string.IsNullOrEmpty(oldName) ? $"'{oldName}'" : $"'{name}'";
                                GlobalService.RecordActivity(
                                    "Update",
                                    "Cập nhật món",
                                    $"{itemLabel}: {string.Join("; ", changes)}");
                            }
                            else
                            {
                                // Nothing actually changed; avoid noisy activity entry
                                // (optional) you may choose to still record a 'no-change' activity
                            }
                        }
                    }
                    catch
                    {
                        // Swallow exceptions from activity recording to avoid breaking save flow
                    }

                    JetMoonMessageBox.Show("Lưu thông tin món thành công!", "Hoàn tất", MsgType.Success);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    JetMoonMessageBox.Show("Không thể lưu dữ liệu. Vui lòng thử lại.", "Lỗi", MsgType.Error);
                }
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi hệ thống:\n" + ex.Message, "Crash Report", MsgType.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}