using Microsoft.Win32;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Models; // Hoặc QuanLyCaPhe nếu Class Table ở ngoài
using QuanLyCaPhe.Views.Admin.DetailWindow;
using QuanLyCaPhe.Views.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class TablePage : Page
    {
        public List<Table> TableList { get; set; }

        public TablePage()
        {
            InitializeComponent();
            Loaded += TablePage_Loaded;
        }

        private void TablePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                TableList = TableDAO.Instance.LoadTableList();
                Tablesdg.ItemsSource = TableList;
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MsgType.Error);
            }
        }

        private void Tablesdg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void SearchBar_Clicked(object sender, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                Tablesdg.ItemsSource = TableList;
            }
            else
            {
                // Tìm kiếm theo Name
                var filtered = TableList.Where(t =>
                    (t.Name != null && t.Name.ToLower().Contains(keyword.ToLower())) ||
                    (t.Status != null && t.Status.ToLower().Contains(keyword.ToLower()))
                ).ToList();
                Tablesdg.ItemsSource = filtered;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var f = new TableDetailWindow();
            if (f.ShowDialog() == true) LoadData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedTable = menuItem?.DataContext as Table;

            if (selectedTable == null)
            {
                JetMoonMessageBox.Show("Vui lòng chọn bàn cần sửa!", "Lỗi", MsgType.Warning);
                return;
            }

            var f = new TableDetailWindow(selectedTable);
            if (f.ShowDialog() == true) LoadData();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedTable = menuItem?.DataContext as Table;

            if (selectedTable == null) return;

            var result = JetMoonMessageBox.Show(
                $"Bạn có chắc muốn xóa '{selectedTable.Name}' không?",
                "Xác nhận xóa",
                MsgType.Question, true);

            if (result == true)
            {
                if (TableDAO.Instance.DeleteTable(selectedTable.Id))
                {
                    JetMoonMessageBox.Show("Đã xóa thành công!", "Hoàn tất", MsgType.Success);
                    LoadData();
                }
                else
                {
                    JetMoonMessageBox.Show("Không thể xóa bàn này.", "Lỗi", MsgType.Error);
                }
            }
        }

        private void IETable_Clicked(object sender, string e)
        {
            if (e == "Export")
            {
                var sfd = new SaveFileDialog() { Filter = "Excel|*.xlsx", FileName = "DS_Ban.xlsx" };
                if (sfd.ShowDialog() == true)
                {
                    ExcelHelper.ExportList<Table>(sfd.FileName, TableList, "Tables");
                    JetMoonMessageBox.Show("Xuất thành công!", "Thông báo", MsgType.Success);
                }
            }
        }
    }
}