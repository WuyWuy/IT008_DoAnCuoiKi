using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;


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
            string email = txtEmail.Text;
            string phone = txtPhone.Text;
            string address = txtAddress.Text;
            string gender = cboGender.Text;
            string role = (cboRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Staff";

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

                    if (UserDAO.Instance.InsertUser(name, email, phone, address, gender, password, role))
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

                    if (UserDAO.Instance.UpdateUser(_user.Id, name, phone, address, gender, role))
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
