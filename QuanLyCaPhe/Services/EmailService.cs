using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;

namespace QuanLyCaPhe.Services
{
    public class EmailService
    {
        private static EmailService _instance;
        public static EmailService Instance => _instance ??= new EmailService();

        private const string SENDER_EMAIL = "liminhthuyen@gmail.com";
        private const string SENDER_PASSWORD = "cnhyepmtkxihmoal"; 

        private EmailService() { }

        public async Task<bool> SendOTPEmailAsync(string toEmail, string otpCode)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(SENDER_EMAIL, SENDER_PASSWORD),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SENDER_EMAIL, "JetMoon Coffee Support"),
                    Subject = "Mã xác nhận khôi phục mật khẩu",
                    Body = $@"
                        <h2>Yêu cầu cấp lại mật khẩu</h2>
                        <p>Xin chào,</p>
                        <p>Mã xác nhận (OTP) của bạn là: <b style='font-size:20px;color:blue;'>{otpCode}</b></p>
                        <p>Mã này sẽ hết hạn trong vòng 5 phút.</p>
                        <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception e)
            {
                // Ghi log lỗi nếu cần
                MessageBox.Show(e.Message);
                return false;
            }
        }
    }
}