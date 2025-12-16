using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
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
 MessageBox.Show("Ch?n nguyên li?u");
 return;
 }
 if (!double.TryParse(tbAmount.Text, out double amt) || amt <=0)
 {
 MessageBox.Show("S? l??ng không h?p l?");
 return;
 }
 var ing = cbIngredients.SelectedItem as Ingredient;
 if (ing == null) return;
 if (RecipeDAO.Instance.InsertRecipe(_productId, ing.Id, amt))
 {
 this.DialogResult = true;
 this.Close();
 }
 else
 {
 MessageBox.Show("Không th? thêm công th?c");
 }
 }

 private void btnCancel_Click(object sender, RoutedEventArgs e)
 {
 this.DialogResult = false;
 this.Close();
 }
 }
}
