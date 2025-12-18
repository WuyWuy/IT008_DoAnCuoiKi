using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class RecipeEditorWindow : Window
    {
        private int _productId;
        private List<RecipeViewModel> _items;

        public class RecipeViewModel
        {
            public int Id { get; set; }
            public int ProId { get; set; }
            public int IngId { get; set; }
            public string IngName { get; set; } = string.Empty;
            public double Amount { get; set; }
            public string Unit { get; set; } = string.Empty;
        }

        public RecipeEditorWindow(int productId, string productName)
        {
            InitializeComponent();
            _productId = productId;
            // Sửa lại tiêu đề không dấu thành có dấu
            tbTitle.Text = $"Công thức pha chế: {productName}";
            LoadRecipes();
        }

        private void LoadRecipes()
        {
            var recs = RecipeDAO.Instance.GetListRecipeByProID(_productId);
            var ings = IngredientDAO.Instance.GetListIngredient();

            _items = recs.Select(r => new RecipeViewModel
            {
                Id = r.Id,
                ProId = r.ProId,
                IngId = r.IngId,
                IngName = ings.FirstOrDefault(i => i.Id == r.IngId)?.IngName ?? "(Không xác định)",
                Amount = r.Amount,
                Unit = ings.FirstOrDefault(i => i.Id == r.IngId)?.Unit ?? string.Empty
            }).ToList();

            dgRecipe.ItemsSource = _items;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RecipeAddEditWindow(_productId);
            if (dialog.ShowDialog() == true)
            {
                LoadRecipes();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var sel = dgRecipe.SelectedItem as RecipeViewModel;
            if (sel == null)
            {
                JetMoonMessageBox.Show("Vui lòng chọn dòng cần xóa!", "Chưa chọn", MsgType.Warning);
                return;
            }

            // Dùng JetMoonMessageBox kiểu Question
            var res = JetMoonMessageBox.Show(
                $"Bạn có chắc muốn xóa nguyên liệu '{sel.IngName}' khỏi công thức?",
                "Xác nhận xóa",
                MsgType.Question,
                true); // true = Hiện nút Cancel

            if (res != true) return;

            if (RecipeDAO.Instance.DeleteRecipe(sel.Id))
            {
                LoadRecipes();
                JetMoonMessageBox.Show("Đã xóa thành công!", "Hoàn tất", MsgType.Success);
            }
            else
            {
                JetMoonMessageBox.Show("Xóa thất bại. Vui lòng thử lại.", "Lỗi", MsgType.Error);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}