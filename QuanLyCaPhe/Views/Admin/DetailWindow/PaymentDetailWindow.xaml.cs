using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class PaymentDetailWindow : Window
    {
        private PaymentAccount _account;

        public PaymentDetailWindow(PaymentAccount account = null)
        {
            InitializeComponent();
            _account = account;

            if (_account != null)
            {
                // Load dữ liệu cũ lên form nếu là chế độ Sửa
                txtBankName.Text = _account.BankName;
                txtBin.Text = _account.BankBin;
                txtAccNum.Text = _account.AccountNumber;
                txtAccName.Text = _account.AccountName;

                // Chọn đúng Template cũ
                foreach (ComboBoxItem item in cboTemplate.Items)
                {
                    if (item.Tag.ToString() == _account.Template)
                    {
                        cboTemplate.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (string.IsNullOrWhiteSpace(txtBankName.Text) ||
                string.IsNullOrWhiteSpace(txtBin.Text) ||
                string.IsNullOrWhiteSpace(txtAccNum.Text) ||
                string.IsNullOrWhiteSpace(txtAccName.Text))
            {
                JetMoonMessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thiếu dữ liệu", MsgType.Warning);
                return;
            }

            string bank = txtBankName.Text.Trim();
            string bin = txtBin.Text.Trim();
            string accNo = txtAccNum.Text.Trim();
            string accName = txtAccName.Text.Trim().ToUpper();
            string template = (cboTemplate.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "compact2";

            try
            {
                if (_account == null) // THÊM MỚI
                {
                    if (PaymentAccountDAO.Instance.Insert(bank, bin, accNo, accName, template))
                    {
                        JetMoonMessageBox.Show("Thêm tài khoản thành công!", "Hoàn tất", MsgType.Success);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Thêm thất bại. Vui lòng thử lại!", "Lỗi", MsgType.Error);
                    }
                }
                else // CẬP NHẬT
                {
                    if (PaymentAccountDAO.Instance.Update(_account.Id, bank, bin, accNo, accName, template))
                    {
                        JetMoonMessageBox.Show("Cập nhật thông tin thành công!", "Hoàn tất", MsgType.Success);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Cập nhật thất bại!", "Lỗi", MsgType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MsgType.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}