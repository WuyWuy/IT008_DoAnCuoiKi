using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import Custom MessageBox
using System;
using System.Windows;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class IngredientDetailWindow : Window
    {
        private Ingredient _ingredient;

        public IngredientDetailWindow(Ingredient ingredient = null)
        {
            InitializeComponent();
            _ingredient = ingredient;

            if (_ingredient != null)
            {
                txtIngName.Text = _ingredient.IngName;
                txtUnit.Text = _ingredient.Unit;
                txtQuantity.Text = _ingredient.Quantity.ToString();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate Tên và Đơn vị
            if (string.IsNullOrWhiteSpace(txtIngName.Text) || string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                JetMoonMessageBox.Show("Vui lòng nhập tên nguyên liệu và đơn vị tính!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            string name = txtIngName.Text.Trim();
            string unit = txtUnit.Text.Trim();

            // Validate Số lượng
            if (!double.TryParse(txtQuantity.Text, out double quantity))
            {
                JetMoonMessageBox.Show("Số lượng tồn kho phải là một con số!", "Lỗi nhập liệu", MsgType.Error);
                return;
            }

            if (quantity < 0)
            {
                JetMoonMessageBox.Show("Số lượng tồn kho không được âm!", "Lỗi nhập liệu", MsgType.Error);
                return;
            }

            try
            {
                if (_ingredient == null) // Thêm mới
                {
                    // Kiểm tra trùng tên trước khi thêm (Optional - Good UX)
                    if (IngredientDAO.Instance.GetIdByName(name) != -1)
                    {
                        JetMoonMessageBox.Show("Tên nguyên liệu này đã tồn tại!", "Trùng lặp", MsgType.Warning);
                        return;
                    }

                    if (IngredientDAO.Instance.InsertIngredient(name, unit, quantity))
                    {
                        JetMoonMessageBox.Show("Thêm nguyên liệu thành công!", "Hoàn tất", MsgType.Success);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Thêm nguyên liệu thất bại!", "Lỗi", MsgType.Error);
                    }
                }
                else // Cập nhật
                {
                    if (IngredientDAO.Instance.UpdateIngredient(_ingredient.Id, name, unit, quantity))
                    {
                        JetMoonMessageBox.Show("Cập nhật nguyên liệu thành công!", "Hoàn tất", MsgType.Success);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Cập nhật thất bại!", "Lỗi", MsgType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MsgType.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}