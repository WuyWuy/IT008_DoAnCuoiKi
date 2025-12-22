using Microsoft.Win32;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Services; // [QUAN TRỌNG] Để gọi CheckWarnings
using QuanLyCaPhe.Views.Admin.DetailWindow;
using QuanLyCaPhe.Views.Components; // [QUAN TRỌNG] Để dùng JetMoonMessageBox
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class InputInfoPage : Page
    {
        public InputInfoPage()
        {
            InitializeComponent();
            Loaded += InputInfoPage_Loaded;
        }

        private void InputInfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            InputInfosdg.ItemsSource = InputInfoDAO.Instance.GetListInputInfo();
        }

        // --- Đếm STT ---
        private void Productsdg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        // --- 1. THÊM PHIẾU NHẬP ---
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            InputInfoDetailWindow f = new InputInfoDetailWindow();
            if (f.ShowDialog() == true)
            {
                // [MỚI] Ghi log sau khi thêm thành công
                GlobalService.RecordActivity("Kho hàng", "Nhập hàng", "Vừa thêm phiếu nhập hàng mới");

                LoadData();
            }
        }

        // --- 2. SỬA PHIẾU NHẬP ---
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (InputInfosdg.SelectedItem is InputInfo selected)
            {
                InputInfoDetailWindow f = new InputInfoDetailWindow(selected);
                if (f.ShowDialog() == true)
                {
                    // [MỚI] Ghi log sau khi sửa thành công
                    GlobalService.RecordActivity("Kho hàng", "Sửa phiếu nhập", $"Đã cập nhật thông tin phiếu nhập #{selected.Id}");

                    LoadData();
                }
            }
            else
            {
                JetMoonMessageBox.Show("Vui lòng chọn phiếu cần xem/sửa!", "Chưa chọn dòng", MsgType.Warning);
            }
        }

        // --- 3. XÓA PHIẾU NHẬP ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (InputInfosdg.SelectedItem is InputInfo selected)
            {
                // Hỏi xác nhận
                var result = JetMoonMessageBox.Show(
                    $"Bạn có chắc muốn xóa phiếu nhập #{selected.Id}?\n" +
                    $"\nLƯU Ý: Số lượng tồn kho của [{selected.IngredientName}] sẽ bị trừ đi {selected.Count} {selected.Unit}.",
                    "Cảnh báo kho",
                    MsgType.Question,
                    true);

                if (result == true)
                {
                    // Thực hiện xóa
                    if (InputInfoDAO.Instance.DeleteInputInfo(selected.Id, selected.IngId, selected.Count))
                    {
                        // Kiểm tra cảnh báo (Warning) ngay lập tức vì kho vừa bị trừ
                        GlobalService.CheckWarnings();

                        // [MỚI] Ghi log hoạt động
                        GlobalService.RecordActivity(
                            "Kho hàng",
                            "Xóa phiếu nhập",
                            $"Đã xóa phiếu nhập #{selected.Id} - {selected.IngredientName} ({selected.Count} {selected.Unit})"
                        );

                        JetMoonMessageBox.Show("Đã xóa phiếu nhập và cập nhật lại kho hàng!", "Thành công", MsgType.Success);
                        LoadData();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Xóa thất bại! Có lỗi xảy ra với CSDL.", "Lỗi", MsgType.Error);
                    }
                }
            }
            else
            {
                JetMoonMessageBox.Show("Vui lòng chọn dòng cần xóa!", "Chưa chọn dòng", MsgType.Warning);
            }
        }

        // --- TÌM KIẾM ---
        private void SearchBar_Clicked(object sender, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                LoadData();
            else
                InputInfosdg.ItemsSource = InputInfoDAO.Instance.SearchInputInfo(keyword);
        }

        // --- EXPORT ---

        private void IETable_Clicked(object sender, string e)
        {
            if (e == "Export") ExportData();
        }
        private void ExportData()
        {
            var sfd = new SaveFileDialog() { Filter = "Excel|*.xlsx", FileName = "DS_Hanghoa.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                var list = InputInfoDAO.Instance.GetListInputInfo();
                ExcelHelper.ExportList<InputInfo>(sfd.FileName, list, "HangHoas");
                JetMoonMessageBox.Show("Xuất xong!", "Thông báo", MsgType.Success);
            }
        }
    }
}