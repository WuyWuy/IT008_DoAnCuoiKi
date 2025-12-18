using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import
using System.Linq;
using System.Windows;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class RecipeAddEditWindow : Window
    {
        private int _productId;
        public RecipeAddEditWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            cbIngredients.ItemsSource = IngredientDAO.Instance.GetListIngredient();
            cbIngredients.SelectedIndex = -1;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (cbIngredients.SelectedItem == null)
            {
                // Sửa lỗi font "Ch?n nguyên li?u"
                JetMoonMessageBox.Show("Vui lòng chọn nguyên liệu!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            if (!double.TryParse(tbAmount.Text, out double amt) || amt <= 0)
            {
                // Sửa lỗi font "S? l??ng không h?p l?"
                JetMoonMessageBox.Show("Số lượng (định lượng) không hợp lệ!", "Lỗi nhập liệu", MsgType.Error);
                return;
            }

            var ing = cbIngredients.SelectedItem as Ingredient;
            if (ing == null) return;

            if (RecipeDAO.Instance.InsertRecipe(_productId, ing.Id, amt))
            {
                JetMoonMessageBox.Show("Thêm công thức thành công!", "Hoàn tất", MsgType.Success);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // Sửa lỗi font "Không th? thêm công th?c"
                JetMoonMessageBox.Show("Không thể thêm công thức. Vui lòng kiểm tra lại.", "Lỗi", MsgType.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}