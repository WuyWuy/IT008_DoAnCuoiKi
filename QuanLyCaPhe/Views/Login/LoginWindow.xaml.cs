using QuanLyCaPhe.Views.Admin;
using QuanLyCaPhe.Views.Staff;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Login
{
    public partial class LoginWindow : Window
    {
        private readonly UserStore _store;

        public LoginWindow()
        {
            InitializeComponent();

            _store = new UserStore();

            // ensure table exists
            _store.EnsureUsersTable();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            tbLoginStatus.Text = string.Empty;

            var email = txtLoginEmail.Text.Trim();
            var pwd = pbLoginPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd))
            {
                tbLoginStatus.Text = "Email and password required.";
                return;
            }

            try
            {
                var ok = _store.ValidateUserCredentials(email, pwd);
                if (ok) //________________________________remember delete me !! || email == "admin@test.com"
                {
                    tbLoginStatus.Foreground = System.Windows.Media.Brushes.Green;
                    tbLoginStatus.Text = "Login successful.";
                    // TODO: open main window or proceed;
                    MessageBoxResult result = MessageBox.Show
                    (
                        "Mày là Admin à -_-",
                        "Demo Admin or Staff",
                        MessageBoxButton.YesNo
                    );
                    if (result == MessageBoxResult.Yes)
                    {
                        AdminWindow f = new AdminWindow();
                        f.Show();
                    }
                    else
                    {
                        StaffWindow f = new StaffWindow();
                        f.Show();
                    }    
                    this.Close();
                }
                else
                {
                    tbLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
                    tbLoginStatus.Text = "Invalid email or password.";
                }
            }
            catch (Exception ex)
            {
                tbLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
                tbLoginStatus.Text = "Login failed: " + ex.Message;
            }
        }

        private void BtnClearLogin_Click(object sender, RoutedEventArgs e)
        {
            txtLoginEmail.Clear();
            pbLoginPassword.Clear();
            tbLoginStatus.Text = string.Empty;
        }

        

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            // basic RFC-like validation
            var rx = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
            return rx.IsMatch(email);
        }

        private static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.Length >= 7 && digits.Length <= 15;
        }
    }
}