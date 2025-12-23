using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin;
using QuanLyCaPhe.Views.Staff;
using System;
using System.Windows;
using System.Windows.Input;

namespace QuanLyCaPhe.Views.Login
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();


            this.MouseDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) this.DragMove(); };
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                tbStatus.Text = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
                return;
            }

            try
            {
                if (UserDAO.Instance.Login(email, password))
                {
                    tbStatus.Text = "";

                    int userId = UserDAO.Instance.GetIdByEmail(email);

                    // Store current user info globally so other windows can access
                    try
                    {
                        Application.Current.Properties["CurrentUserId"] = userId;
                        Application.Current.Properties["CurrentUserName"] = UserDAO.Instance.GetFullName(userId);
                    }
                    catch { }

                    string role = UserDAO.Instance.GetUserRole(userId); 

                    Hide();

                    if (role == "Admin")
                    {
                        AdminWindow adminWin = new AdminWindow();
                        adminWin.Show();
                    }
                    else
                    {
                        StaffWindow staffWin = new StaffWindow();
                        staffWin.Show();
                    }

                }
                else
                {
                    tbStatus.Text = "Email hoặc mật khẩu không chính xác!";
                }
            }
            catch (Exception ex)
            {
                tbStatus.Text = "Lỗi kết nối: " + ex.Message;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var forgotWindow = new QuanLyCaPhe.Views.Login.ForgotPasswordWindow();
            forgotWindow.ShowDialog();
        }
    }
}