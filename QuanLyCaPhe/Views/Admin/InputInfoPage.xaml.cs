using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin.DetailWindow;
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

        // --- THÊM PHIẾU NHẬP ---
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            InputInfoDetailWindow f = new InputInfoDetailWindow();
            // Nếu người dùng bấm Lưu và đóng form -> Load lại danh sách
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- SỬA PHIẾU NHẬP (Chỉ cho xem/sửa giá/ngày, hạn chế sửa số lượng vì rắc rối kho) ---
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // InputInfoDetailWindow của bạn đã hỗ trợ nhận tham số InputInfo vào Constructor
            // Hãy đảm bảo bạn đã code phần đó (như tôi đã gửi ở các bước trước)
            if (InputInfosdg.SelectedItem is InputInfo selected)
            {
                InputInfoDetailWindow f = new InputInfoDetailWindow(selected);
                if (f.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        // --- XÓA PHIẾU NHẬP ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (InputInfosdg.SelectedItem is InputInfo selected)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa phiếu nhập #{selected.Id}?\n\nLƯU Ý: Số lượng tồn kho của [{selected.IngredientName}] sẽ bị trừ đi {selected.Count}.",
                    "Cảnh báo kho",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (InputInfoDAO.Instance.DeleteInputInfo(selected.Id, selected.IngId, selected.Count))
                    {
                        MessageBox.Show("Đã xóa và cập nhật lại kho hàng!");
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!");
                    }
                }
            }
        }

        // --- TÌM KIẾM ---
        // Bạn cần gán sự kiện này vào SearchBar trong XAML: Clicked="SearchBar_Clicked"
        private void SearchBar_Clicked(object sender, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                LoadData();
            else
                InputInfosdg.ItemsSource = InputInfoDAO.Instance.SearchInputInfo(keyword);
        }
    }
}