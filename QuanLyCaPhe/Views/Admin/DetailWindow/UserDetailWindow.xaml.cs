using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class UserDetailWindow : Window
    {
        private User _user;

        public UserDetailWindow()
        {
            InitializeComponent();
            _user = null;
            txtEmail.IsEnabled = true;
            pnlPassword.Visibility = Visibility.Visible;
        }

        public UserDetailWindow(User user)
        {
            InitializeComponent();
            _user = user;

            txtFullName.Text = user.FullName;
            txtEmail.Text = user.Email;
            txtPhone.Text = user.Phone;
            txtAddress.Text = user.Address;

            var wageBox = this.FindName("txtHourlyWage") as TextBox;
            if (wageBox != null)
            {
                wageBox.Text = user.HourlyWage.ToString("N0");
            }

            foreach (ComboBoxItem item in cboRole.Items)
            {
                if (item.Content.ToString() == user.RoleName)
                {
                    cboRole.SelectedItem = item;
                    break;
                }
            }

            txtEmail.IsEnabled = false;
            pnlPassword.Visibility = Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                JetMoonMessageBox.Show("Vui lòng nhập tên và email!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            string name = txtFullName.Text;
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text;
            string address = txtAddress.Text;
            string gender = cboGender.Text;
            string role = (cboRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Staff";

            decimal hourly = 0m;
            var wageBox = this.FindName("txtHourlyWage") as TextBox;
            string wageText = wageBox?.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(wageText))
            {
                if (!decimal.TryParse(wageText, out hourly))
                {
                    JetMoonMessageBox.Show("Lương theo giờ không hợp lệ. Vui lòng nhập số.", "Lỗi nhập liệu", MsgType.Error);
                    return;
                }
            }

            const string emailPattern = "^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                JetMoonMessageBox.Show("Email không đúng định dạng (xxx@xxx.com).", "Lỗi định dạng", MsgType.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                if (_user == null)
                {
                    if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        JetMoonMessageBox.Show("Vui lòng nhập mật khẩu khởi tạo!", "Thiếu thông tin", MsgType.Warning);
                        return;
                    }

                    if (UserDAO.Instance.GetIdByEmail(email) != -1)
                    {
                        JetMoonMessageBox.Show("Email này đã tồn tại trong hệ thống!", "Trùng lặp", MsgType.Error);
                        return;
                    }

                    string password = txtPassword.Password; // Thực tế nên hash password ở đây trước khi gửi xuống DAO nếu DAO chưa hash

                    if (UserDAO.Instance.InsertUser(name, email, phone, address, gender, password, role, hourly))
                    {
                        JetMoonMessageBox.Show("Thêm nhân viên thành công!", "Hoàn tất", MsgType.Success);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Thêm nhân viên thất bại!", "Lỗi", MsgType.Error);
                    }
                }
                else
                {
                    if (UserDAO.Instance.UpdateUser(_user.Id, name, phone, address, gender, role, hourly))
                    {
                        JetMoonMessageBox.Show("Cập nhật hồ sơ thành công!", "Hoàn tất", MsgType.Success);
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

        // Helper functions (Giữ nguyên)
        private string CreateSalt()
        {
            byte[] bytes = new byte[128 / 8];
            using (var keyGenerator = RandomNumberGenerator.Create())
            {
                keyGenerator.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}