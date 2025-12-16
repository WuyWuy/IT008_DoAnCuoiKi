using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
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
 tbTitle.Text = $"Cong thuc: {productName}";
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
 IngName = ings.FirstOrDefault(i => i.Id == r.IngId)?.IngName ?? "(Khong xac dinh)",
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
 if (sel == null) return;
 var res = MessageBox.Show($"Xoa cong thuc cho '{sel.IngName}'?", "Xac nhan", MessageBoxButton.YesNo, MessageBoxImage.Question);
 if (res != MessageBoxResult.Yes) return;
 if (RecipeDAO.Instance.DeleteRecipe(sel.Id))
 {
 LoadRecipes();
 }
 else
 {
 MessageBox.Show("Xoa that bai.");
 }
 }

 private void btnClose_Click(object sender, RoutedEventArgs e)
 {
 this.DialogResult = true;
 this.Close();
 }
 }
}
