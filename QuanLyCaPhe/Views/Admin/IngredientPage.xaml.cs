using Microsoft.Win32;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using QuanLyCaPhe.Views.Components; // [QUAN TRỌNG] Import để dùng JetMoonMessageBox
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
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

        // --- Sửa Nguyên Liệu ---
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedIngredient = menuItem?.DataContext as Ingredient;

            if (selectedIngredient == null)
            {
                JetMoonMessageBox.Show("Vui lòng chọn nguyên liệu cần sửa!", "Chưa chọn dòng", MsgType.Warning);
                return;
            }

            var f = new IngredientDetailWindow(selectedIngredient);
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Xóa Nguyên Liệu (Đã hoàn thiện) ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedIngredient = menuItem?.DataContext as Ingredient;

            if (selectedIngredient == null) return;

            // 1. Hỏi xác nhận
            var result = JetMoonMessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nguyên liệu '{selectedIngredient.IngName}' không?\n" +
                $"Lưu ý: Lịch sử nhập hàng của nguyên liệu này cũng sẽ bị xóa.",
                "Xác nhận xóa",
                MsgType.Question,
                true); // Hiện nút Cancel

            if (result == true)
            {
                try
                {
                    // 2. Gọi DAO xóa
                    bool deleted = IngredientDAO.Instance.DeleteIngredient(selectedIngredient.Id);

                    if (deleted)
                    {
                        JetMoonMessageBox.Show("Đã xóa thành công!", "Hoàn tất", MsgType.Success);
                        LoadData();
                    }
                    else
                    {
                        // Trường hợp không xóa được do ràng buộc (đang dùng trong công thức)
                        JetMoonMessageBox.Show(
                            "Không thể xóa nguyên liệu này vì đang được sử dụng trong công thức món ăn.\n" +
                            "Vui lòng gỡ nguyên liệu khỏi công thức trước.",
                            "Không thể xóa",
                            MsgType.Warning);
                    }
                }
                catch (Exception ex)
                {
                    JetMoonMessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MsgType.Error);
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
                    JetMoonMessageBox.Show(
                        $"Xử lý xong!\n- Thêm mới: {countAdd} nguyên liệu\n- Cập nhật: {countUpdate} nguyên liệu",
                        "Kết quả Import",
                        MsgType.Success);
                }
                catch (Exception ex)
                {
                    JetMoonMessageBox.Show("Lỗi Import: " + ex.Message, "Lỗi", MsgType.Error);
                }
            }
        }

        // --- Export ---
        private void ExportData()
        {
            var sfd = new SaveFileDialog() { Filter = "Excel|*.xlsx", FileName = "DS_NguyenLieu.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var list = IngredientDAO.Instance.GetListIngredient();
                    ExcelHelper.ExportList<Ingredient>(sfd.FileName, list, "NguyenLieus");
                    JetMoonMessageBox.Show("Xuất dữ liệu thành công!", "Thông báo", MsgType.Success);
                }
                catch (Exception ex)
                {
                    JetMoonMessageBox.Show("Lỗi Export: " + ex.Message, "Lỗi", MsgType.Error);
                }
            }
        }

        private void IETable_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}