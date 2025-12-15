using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
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
            if (string.IsNullOrWhiteSpace(txtIngName.Text) || string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Vui lòng nhập tên và đơn vị tính!");
                return;
            }

            string name = txtIngName.Text;
            string unit = txtUnit.Text;

            if (!double.TryParse(txtQuantity.Text, out double quantity))
            {
                MessageBox.Show("Số lượng phải là số!");
                return;
            }

            try
            {
                if (_ingredient == null) // Thêm mới
                {
                    if (IngredientDAO.Instance.InsertIngredient(name, unit, quantity))
                    {
                        MessageBox.Show("Thêm nguyên liệu thành công!");
                        DialogResult = true;
                        Close();
                    }
                    else MessageBox.Show("Thêm thất bại!");
                }
                else 
                {
                    if (IngredientDAO.Instance.UpdateIngredient(_ingredient.Id, name, unit, quantity))
                    {
                        MessageBox.Show("Cập nhật thành công!");
                        DialogResult = true;
                        Close();
                    }
                    else MessageBox.Show("Cập nhật thất bại!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}