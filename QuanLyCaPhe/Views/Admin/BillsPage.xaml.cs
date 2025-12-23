
using Microsoft.Win32;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using QuanLyCaPhe.Views.Components; // [QUAN TRỌNG] Import component
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using QuanLyCaPhe.Services;

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

        // --- XỬ LÝ XÓA HÓA ĐƠN (ĐÃ ĐỒNG BỘ) ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Bill selectedBill = Billsdg.SelectedItem as Bill;
            if (selectedBill == null)
            {
                // [FIXED] Dùng JetMoonMessageBox - Warning
                JetMoonMessageBox.Show("Vui lòng chọn hóa đơn để xóa.", "Chưa chọn", MsgType.Warning);
                return;
            }

            // [FIXED] Dùng JetMoonMessageBox - Question
            var result = JetMoonMessageBox.Show(
                $"Bạn có chắc muốn xóa hóa đơn #{selectedBill.Id}?",
                "Xác nhận xóa",
                MsgType.Question,
                true); // true để hiện nút Cancel/No

            if (result == true)
            {
                if (BillDAO.Instance.DeleteBill(selectedBill.Id))
                {
                    // Record activity of deletion so Activities/Recent list updates
                    try
                    {
                        GlobalService.RecordActivity("Delete", "Xóa hóa đơn", $"Đã xóa hóa đơn #{selectedBill.Id}");
                    }
                    catch
                    {
                        // Swallow to avoid breaking UI if logging fails
                    }

                    // [FIXED] Dùng JetMoonMessageBox - Success
                    JetMoonMessageBox.Show("Đã xóa hóa đơn.", "Thành công", MsgType.Success);
                    LoadData();
                }
                else
                {
                    // [FIXED] Dùng JetMoonMessageBox - Error
                    JetMoonMessageBox.Show("Xóa thất bại. Vui lòng thử lại.", "Lỗi", MsgType.Error);
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

        private void IETable_Clicked(object sender, string e)
        {
            if (e == "Export") ExportData();
        }

        private void ExportData()
        {
            var sfd = new SaveFileDialog() { Filter = "Excel|*.xlsx", FileName = "DS_Hoadon.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                var list = BillDAO.Instance.GetListBills();
                ExcelHelper.ExportList<Bill>(sfd.FileName, list, "Hoadons");
                JetMoonMessageBox.Show("Xuất xong!", "Thông báo", MsgType.Success);
            }
        }
    }
}