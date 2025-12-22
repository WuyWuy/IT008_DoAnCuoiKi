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

        // Public wrapper so other windows/pages can request a refresh
        public void RefreshData()
        {
            LoadData();
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

        // --- XỬ LÝ XÓA HÓA ĐƠN ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Bill selectedBill = Billsdg.SelectedItem as Bill;
            if (selectedBill == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn để xóa.", "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc muốn xóa hóa đơn #{selectedBill.Id}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (BillDAO.Instance.DeleteBill(selectedBill.Id))
                {
                    MessageBox.Show("Đã xóa hóa đơn.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Xóa thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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