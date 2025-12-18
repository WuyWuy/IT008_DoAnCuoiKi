using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
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

            // Use FindName to access the TextBox to avoid generated-field issues
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
                MessageBox.Show("Vui lòng nhập tên và email!");
                return;
            }

            string name = txtFullName.Text;
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text;
            string address = txtAddress.Text;
            string gender = cboGender.Text;
            string role = (cboRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Staff";

            // hourly wage
            decimal hourly =0m;
            var wageBox = this.FindName("txtHourlyWage") as TextBox;
            string wageText = wageBox?.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(wageText))
            {
                if (!decimal.TryParse(wageText, out hourly))
                {
                    MessageBox.Show("Lương theo giờ không hợp lệ. Vui lòng nhập số hợp lệ.");
                    return;
                }
            }

            // Validate email format
            const string emailPattern = "^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng nhập theo định dạng xxx@xxx.com");
                txtEmail.Focus();
                return;
            }

            try
            {
                if (_user == null)
                {
                    // Kiểm tra password
                    if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        MessageBox.Show("Vui lòng nhập mật khẩu khởi tạo!");
                        return;
                    }

                    // Kiểm tra trùng Email
                    if (UserDAO.Instance.GetIdByEmail(email) != -1)
                    {
                        MessageBox.Show("Email này đã tồn tại trong hệ thống!");
                        return;
                    }

                    // Mã hóa mật khẩu
                    string password = txtPassword.Password;

                    if (UserDAO.Instance.InsertUser(name, email, phone, address, gender, password, role, hourly))
                    {
                        MessageBox.Show("Thêm nhân viên thành công!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Thêm thất bại!");
                    }
                }
                else
                {
                    // --- LOGIC CẬP NHẬT ---

                    if (UserDAO.Instance.UpdateUser(_user.Id, name, phone, address, gender, role, hourly))
                    {
                        MessageBox.Show("Cập nhật thành công!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thất bại!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ================= HELPER MÃ HÓA (ĐỂ TẠO PASS ĐĂNG NHẬP ĐƯỢC) =================

        // 1. Hàm tạo Salt ngẫu nhiên
        private string CreateSalt()
        {
            byte[] bytes = new byte[128 / 8];
            using (var keyGenerator = RandomNumberGenerator.Create())
            {
                keyGenerator.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        // 2. Hàm Hash Password (SHA256)
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                // Kết hợp Pass + Salt
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
