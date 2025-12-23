using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Helpers;
using QuanLyCaPhe.Services;
using QuanLyCaPhe.Views.Components;
using System;
using System.Windows;

namespace QuanLyCaPhe.Views.Login
{
    public partial class ForgotPasswordWindow : Window
    {
        private string _serverOTP; // Mã OTP thật sinh ra
        private string _targetEmail; // Email người dùng nhập

        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        // --- BƯỚC 1: GỬI OTP ---
        private async void BtnSendOTP_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                JetMoonMessageBox.Show("Vui lòng nhập email!", "Lỗi", MsgType.Warning);
                return;
            }

            // Kiểm tra Email có tồn tại trong DB không
            int userId = UserDAO.Instance.GetIdByEmail(email);
            if (userId == -1)
            {
                JetMoonMessageBox.Show("Email này không tồn tại trong hệ thống!", "Lỗi", MsgType.Error);
                return;
            }

            // UI Loading
            progressBar.Visibility = Visibility.Visible;
            pnlSendMail.IsEnabled = false;

            // Sinh mã OTP
            _serverOTP = OTPHelper.GenerateOTP();
            _targetEmail = email;

            // Gửi mail (Chạy bất đồng bộ để không đơ ứng dụng)
            bool isSent = await EmailService.Instance.SendOTPEmailAsync(_targetEmail, _serverOTP);

            progressBar.Visibility = Visibility.Collapsed;
            pnlSendMail.IsEnabled = true;

            if (isSent)
            {
                // Chuyển sang màn hình nhập OTP
                pnlSendMail.Visibility = Visibility.Collapsed;
                pnlVerifyOTP.Visibility = Visibility.Visible;
            }
            else
            {
                JetMoonMessageBox.Show("Không thể gửi email. Vui lòng kiểm tra lại mạng!", "Lỗi", MsgType.Error);
            }
        }

        // --- BƯỚC 2: XÁC NHẬN OTP ---
        private void BtnVerifyOTP_Click(object sender, RoutedEventArgs e)
        {
            string userOTP = txtOTP.Text.Trim();
            if (userOTP == _serverOTP)
            {
                // OTP Đúng -> Chuyển sang màn đổi pass
                pnlVerifyOTP.Visibility = Visibility.Collapsed;
                pnlNewPass.Visibility = Visibility.Visible;
            }
            else
            {
                JetMoonMessageBox.Show("Mã OTP không chính xác!", "Lỗi", MsgType.Error);
            }
        }

        // --- BƯỚC 3: ĐỔI MẬT KHẨU ---
        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            string newPass = txtNewPass.Password;
            string confirmPass = txtConfirmPass.Password;

            if (string.IsNullOrWhiteSpace(newPass))
            {
                JetMoonMessageBox.Show("Vui lòng nhập mật khẩu mới!", "Lỗi", MsgType.Warning);
                return;
            }
            if (newPass != confirmPass)
            {
                JetMoonMessageBox.Show("Mật khẩu nhập lại không khớp!", "Lỗi", MsgType.Warning);
                return;
            }

            // Cập nhật Database
            int userId = UserDAO.Instance.GetIdByEmail(_targetEmail);
            if (UserDAO.Instance.UpdatePassword(userId, newPass))
            {
                JetMoonMessageBox.Show("Đổi mật khẩu thành công! Hãy đăng nhập lại.", "Hoàn tất", MsgType.Success);
                this.Close();
            }
            else
            {
                JetMoonMessageBox.Show("Lỗi CSDL khi cập nhật mật khẩu.", "Lỗi", MsgType.Error);
            }
        }

        // Gửi lại mã
        private async void ResendOTP_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            _serverOTP = OTPHelper.GenerateOTP(); // Sinh mã mới
            await EmailService.Instance.SendOTPEmailAsync(_targetEmail, _serverOTP);
            progressBar.Visibility = Visibility.Collapsed;
            JetMoonMessageBox.Show("Đã gửi lại mã OTP mới!", "Thông báo", MsgType.Success);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}