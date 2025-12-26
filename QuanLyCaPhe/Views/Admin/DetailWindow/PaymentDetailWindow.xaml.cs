// Updated selection handler and GetBinForBank normalization/fallback
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class PaymentDetailWindow : Window
    {
        private PaymentAccount _account;

        // Mapping bank name -> BIN (populated from provided screenshots)
        private static readonly Dictionary<string, string> BankBins = new(StringComparer.OrdinalIgnoreCase)
        {
            {"VietinBank", "970415"},
            {"Vietcombank", "970436"},
            {"BIDV", "970418"},
            {"Agribank", "970405"},
            {"OCB", "970448"},
            {"MBBank", "970422"},
            {"Techcombank", "970407"},
            {"ACB", "970416"},
            {"VPBank", "970432"},
            {"TPBank", "970423"},
            {"Sacombank", "970403"},
            {"HDBank", "970437"},
            {"VietCapitalBank", "970454"},
            {"SCB", "970429"},
            {"VIB", "970441"},
            {"SHB", "970443"},
            {"Eximbank", "970431"},
            {"MSB", "970426"},
            {"CAKE", "546034"},
            {"Ubank", "546035"},
            {"KienLongBank", "970452"},
            {"KBank", "668888"},
            {"MAFC", "977777"},
            {"HongLeong", "970442"},
            {"KEBHANAHN", "970467"},
            {"KEBHanaHCM", "970466"},
            {"Citibank", "533948"},
            {"CBBank", "970444"},
            {"CIMB", "422589"},
            {"DBSBank", "796500"},
            {"Vikki", "970406"},
            {"VBSP", "999888"},
            {"GPBank", "970408"},
            {"KookminHCM", "970463"},
            {"KookminHN", "970462"},
            {"Woori", "970457"},
            {"VRB", "970421"},
            {"HSBC", "458761"},
            {"IBKHN", "970455"},
            {"IBKHCM", "970456"},
            {"ViettelMoney", "971005"},
            {"Timo", "963388"},
            {"VNPTMoney", "971011"},
            {"SaigonBank", "970400"},
            {"BacABank", "970409"},
            {"MoMo", "971025"},
            {"PVcomBank Pay", "971133"},
            {"PVcomBank", "970412"},
            {"MBV", "970414"},
            {"NCB", "970419"},
            {"ShinhanBank", "970424"},
            {"ABBANK", "970425"},
            {"VietABank", "970427"},
            {"NamABank", "970428"},
            {"PGBank", "970430"},
            {"VietBank", "970433"},
            {"BaoVietBank", "970438"},
            {"SeABank", "970440"},
            {"COOPBANK", "970446"},
            {"LPBank", "970449"}
        };

        public PaymentDetailWindow(PaymentAccount account = null)
        {
            InitializeComponent();
            _account = account;

            if (_account != null)
            {
                // Load dữ liệu cũ lên form nếu là chế độ Sửa
                cboBankName.Text = _account.BankName;
                txtBin.Text = GetBinForBank(_account.BankName);
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

        private static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            // remove non-alphanumeric and collapse spaces, lower-case
            var cleaned = Regex.Replace(s, @"[^\p{L}\p{Nd}]+", "").ToLowerInvariant();
            return cleaned;
        }

        private string GetBinForBank(string bankName)
        {
            if (string.IsNullOrWhiteSpace(bankName)) return string.Empty;

            // Try exact dictionary lookup first (case-insensitive)
            if (BankBins.TryGetValue(bankName.Trim(), out var bin)) return bin;

            // Try normalized match (remove spaces/symbols)
            var n = Normalize(bankName);
            if (!string.IsNullOrEmpty(n))
            {
                var match = BankBins.FirstOrDefault(kv => Normalize(kv.Key) == n);
                if (!string.IsNullOrEmpty(match.Value)) return match.Value;
            }

            // Try partial contains heuristics (e.g. user typed "agribank" or "agrib")
            var containMatch = BankBins.FirstOrDefault(kv => Normalize(kv.Key).Contains(n) || n.Contains(Normalize(kv.Key)));
            if (!string.IsNullOrEmpty(containMatch.Value)) return containMatch.Value;

            return string.Empty;
        }

        // Called when user selects an item from the ComboBox
        private void cboBankName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // If selection is from ComboBoxItem that has Tag set, prefer it.
                if (cboBankName.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
                {
                    txtBin.Text = selectedItem.Tag.ToString();
                    return;
                }

                // otherwise fallback to dictionary lookup based on text
                string bank = cboBankName.Text ?? string.Empty;
                txtBin.Text = GetBinForBank(bank);
            }
            catch { }
        }

        // Also update when user finishes typing and leaves the control
        private void cboBankName_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Try to find exact ComboBoxItem by text first (use content match)
                var text = (cboBankName.Text ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    var item = cboBankName.Items.Cast<object>()
                               .OfType<ComboBoxItem>()
                               .FirstOrDefault(i => string.Equals((i.Content?.ToString() ?? "").Trim(), text, StringComparison.OrdinalIgnoreCase));
                    if (item != null && item.Tag != null)
                    {
                        txtBin.Text = item.Tag.ToString();
                        return;
                    }
                }

                // fallback to best-effort mapping
                string bank = cboBankName.Text ?? string.Empty;
                txtBin.Text = GetBinForBank(bank);
            }
            catch { }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (string.IsNullOrWhiteSpace(cboBankName.Text) ||
                string.IsNullOrWhiteSpace(txtBin.Text) ||
                string.IsNullOrWhiteSpace(txtAccNum.Text) ||
                string.IsNullOrWhiteSpace(txtAccName.Text))
            {
                JetMoonMessageBox.Show("Vui lòng điền đầy đủ thông tin! (BIN tự điền khi chọn ngân hàng)", "Thiếu dữ liệu", MsgType.Warning);
                return;
            }

            string bank = cboBankName.Text.Trim();
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