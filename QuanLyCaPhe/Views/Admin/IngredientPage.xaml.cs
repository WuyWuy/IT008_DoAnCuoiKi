using Microsoft.Win32;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for IngredientsPage.xaml
    /// </summary>
    public partial class IngredientPage : Page
    {
        public List<Ingredient> IngredientList { get; set; }

        public IngredientPage()
        {
            InitializeComponent();
            Loaded += IngredientsPage_Loaded;
        }

        private void IngredientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            Ingredientsdg.ItemsSource = IngredientDAO.Instance.GetListIngredient();
        }

        // --- Đếm STT --- 
        private void Ingredientsdg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        // --- Tìm kiếm ---
        private void SearchBar_Clicked(object sender, string e)
        {
            Ingredientsdg.ItemsSource = IngredientDAO.Instance.SearchIngredientByName(SearchBar.Text);
        }

        // --- Thêm Mới ---
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var f = new IngredientDetailWindow();
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Sửa Món ---
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedIngredient = menuItem?.DataContext as Ingredient;

            if (selectedIngredient == null)
            {
                MessageBox.Show("Vui lòng chọn món cần sửa!");
                return;
            }

            var f = new IngredientDetailWindow(selectedIngredient);
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Xóa Món ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedIngredient = menuItem?.DataContext as Ingredient;

            if (selectedIngredient == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa nguyên liệu '{selectedIngredient.IngName}' không?",
                                         "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IngredientDAO.Instance.DeleteIngredient(selectedIngredient.Id);
                    MessageBox.Show("Đã xóa thành công!");
                    LoadData();
                }
                catch
                {
                    MessageBox.Show("Không thể xóa nguyên liệu này.",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    List<Ingredient> importedList = ExcelHelper.ImportList<Ingredient>(ofd.FileName);

                    int countAdd = 0;
                    int countUpdate = 0;

                    foreach (var item in importedList)
                    {
                        string excelName = item.IngName.Trim();
                        int existingId = IngredientDAO.Instance.GetIdByName(excelName);

                        if (existingId != -1)
                        {
                            if (IngredientDAO.Instance.UpdateIngredient(existingId, excelName, item.Unit, item.Quantity))
                                countUpdate++;
                        }
                        else
                        {
                            if (IngredientDAO.Instance.InsertIngredient(excelName, item.Unit, item.Quantity))
                                countAdd++;
                        }
                    }

                    LoadData();
                    MessageBox.Show($"Xử lý xong!\n- Thêm mới: {countAdd} nguyên liệu\n- Cập nhật : {countUpdate} nguyên liệu", "Kết quả");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
        // --- Export ---
        private void ExportData()
        {
            var sfd = new SaveFileDialog() { Filter = "Excel|*.xlsx", FileName = "DS_NguyenLieu.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                var list = IngredientDAO.Instance.GetListIngredient();
                ExcelHelper.ExportList<Ingredient>(sfd.FileName, list, "NguyenLieus");
                MessageBox.Show("Xuất xong!");
            }
        }
    }
}
