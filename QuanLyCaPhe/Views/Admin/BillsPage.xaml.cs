using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class BillsPage : Page
    {
        public BillsPage()
        {
            InitializeComponent();
            Loaded += BillsPage_Loaded;
        }

        private void BillsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            // Gọi hàm lấy danh sách từ DAO
            Billsdg.ItemsSource = BillDAO.Instance.GetListBills();
        }

        // --- XỬ LÝ MENU CHUỘT PHẢI "XEM CHI TIẾT" ---
        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dòng đang chọn trong DataGrid
            Bill selectedBill = Billsdg.SelectedItem as Bill;

            if (selectedBill != null)
            {
                // Mở cửa sổ Detail và truyền object Bill vào
                BillDetailWindow f = new BillDetailWindow(selectedBill);
                f.ShowDialog();
            }
        }

        // --- XỬ LÝ TÌM KIẾM ---
        private void SearchBar_Clicked(object sender, string keyword)
        {
            Billsdg.ItemsSource = BillDAO.Instance.SearchBill(keyword);
        }

        private void Billsdg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}