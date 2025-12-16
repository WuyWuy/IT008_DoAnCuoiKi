using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System.Data;
using System.Windows;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class BillDetailWindow : Window
    {
        // Constructor nhận vào một Bill
        public BillDetailWindow(Bill bill)
        {
            InitializeComponent();

            if (bill != null)
            {
                // 1. Điền thông tin chung
                txtId.Text = "#" + bill.Id.ToString();
                txtDateCheckIn.Text = bill.DateCheckIn.ToString("dd/MM/yyyy HH:mm");
                txtDiscount.Text = bill.Discount.ToString();
                lblTotalPrice.Text = string.Format("{0:N0} VNĐ", bill.TotalPrice);

                // 2. Điền thông tin Bàn & Nhân Viên (Lấy từ properties mở rộng của Bill)
                // Lưu ý: Cần đảm bảo class Bill của bạn đã có TableName và StaffName
                txtTableName.Text = bill.TableName;
                txtUser.Text = bill.StaffName;

                // 3. Load danh sách món ăn
                LoadBillInfo(bill.Id);
            }
        }

        private void LoadBillInfo(int billId)
        {
            try
            {
                DataTable data = BillDAO.Instance.GetBillDetails(billId);
                dgBillInfo.ItemsSource = data.DefaultView;
            }
            catch
            {
                MessageBox.Show("Không thể tải chi tiết món ăn.");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}