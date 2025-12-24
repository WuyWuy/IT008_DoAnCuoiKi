using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using QuanLyCaPhe.Views.Components;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class PaymentPage : Page
    {
        public PaymentPage()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            // ItemsControl nhận List làm nguồn dữ liệu
            icPayments.ItemsSource = PaymentAccountDAO.Instance.GetList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ Detail ở chế độ Thêm mới (không truyền tham số)
            var f = new PaymentDetailWindow();
            if (f.ShowDialog() == true)
            {
                LoadData(); // Load lại danh sách sau khi thêm
            }
        }

        private void BtnOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // 2. Hàm Sửa (Mở cửa sổ Detail với dữ liệu cũ)
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu từ DataContext của MenuItem (chính là PaymentAccount)
            if (sender is FrameworkElement element && element.DataContext is PaymentAccount acc)
            {
                if (acc.IsActive)
                {
                    JetMoonMessageBox.Show("Không thể sửa tài khoản đang sử dụng!", "Cảnh báo", MsgType.Error);
                    return;
                }

                var f = new PaymentDetailWindow(acc); // Truyền tài khoản vào để sửa
                if (f.ShowDialog() == true)
                {
                    LoadData(); // Load lại danh sách sau khi sửa xong
                }
            }
        }

        // 3. Cập nhật lại Hàm Xóa (Để tương thích với cả nút cũ và menu mới)
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy PaymentAccount từ DataContext (cách này an toàn hơn dùng Tag)
            if (sender is FrameworkElement element && element.DataContext is PaymentAccount acc)
            {
                // Logic kiểm tra
                if (acc.IsActive)
                {
                    JetMoonMessageBox.Show("Không thể xóa tài khoản đang sử dụng!", "Cảnh báo", MsgType.Error);
                    return;
                }

                if (JetMoonMessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản {acc.BankName} không?", "Xác nhận xóa", MsgType.Question, true) == true)
                {
                    PaymentAccountDAO.Instance.Delete(acc.Id);
                    LoadData();
                }
            }
        }

        private void BtnActive_Click(object sender, RoutedEventArgs e)
        {
            // Lấy ID từ Tag của nút bấm
            if (sender is Button btn && btn.Tag is int id)
            {
                if (PaymentAccountDAO.Instance.SetActive(id))
                {
                    LoadData(); // Load lại để cập nhật giao diện (cái cũ mất màu xanh, cái mới hiện màu xanh)
                    JetMoonMessageBox.Show("Đã đổi tài khoản thanh toán thành công!", "Thành công", MsgType.Success);
                }
            }
        }
    }
}