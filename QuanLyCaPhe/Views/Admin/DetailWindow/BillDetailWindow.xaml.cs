using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import
using System;
using System.Data;
using System.Windows;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class BillDetailWindow : Window
    {
        public BillDetailWindow(Bill bill)
        {
            InitializeComponent();

            if (bill != null)
            {
                txtId.Text = "#" + bill.Id.ToString();
                txtDateCheckIn.Text = bill.DateCheckIn.ToString("dd/MM/yyyy HH:mm");

                // Show stored TimeUsedHours (hours value selected at payment) — not a datetime
                if (bill.TimeUsedHours.HasValue)
                    txtTimeUsedHours.Text = $"{bill.TimeUsedHours.Value} giờ";
                else
                    txtTimeUsedHours.Text = string.Empty;

                lblTotalPrice.Text = string.Format("{0:N0} VNĐ", bill.TotalPrice);

                txtTableName.Text = bill.TableName;
                txtUser.Text = bill.StaffName;

                // Payment method is readonly text populated from DB
                txtPaymentMethod.Text = string.IsNullOrWhiteSpace(bill.PaymentMethod) ? string.Empty : bill.PaymentMethod;

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
                JetMoonMessageBox.Show("Không thể tải danh sách món ăn của hóa đơn này.", "Lỗi tải dữ liệu", MsgType.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}