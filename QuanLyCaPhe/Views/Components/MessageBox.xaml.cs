using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace QuanLyCaPhe.Views.Components
{
    public enum MsgType { Info, Success, Warning, Error, Question }

    public partial class JetMoonMessageBox : Window
    {
        // SelectedOption stores which labeled button the user clicked (null means closed without choice)
        public string? SelectedOption { get; private set; }

        public JetMoonMessageBox()
        {
            InitializeComponent();
        }

        // Kéo thả cửa sổ (Drag Move)
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // Treat OK as a chosen option (label may be customized)
            SelectedOption = btnOK.Content?.ToString();
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Cancel means user explicitly cancelled -> leave SelectedOption null
            SelectedOption = null;
            DialogResult = false;
            Close();
        }

        // New handler for the middle/alternative primary option (e.g. "Chuyển khoản")
        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = btnAlt.Content?.ToString();
            DialogResult = true;
            Close();
        }

        // STATIC METHOD ĐỂ GỌI NHANH (Giống MessageBox.Show)
        public static bool? Show(string message, string title = "Thông báo", MsgType type = MsgType.Info, bool showCancel = false)
        {
            var msg = new JetMoonMessageBox();
            msg.txtMessage.Text = message;
            msg.txtTitle.Text = title;

            // Ensure the alternate button is hidden for simple Show
            msg.btnAlt.Visibility = Visibility.Collapsed;

            // Xử lý hiển thị nút Cancel
            if (showCancel)
            {
                msg.btnCancel.Visibility = Visibility.Visible;
                msg.btnOK.Content = "Đồng ý"; // Hoặc "Xác nhận"
            }
            else
            {
                msg.btnCancel.Visibility = Visibility.Collapsed;
                msg.btnOK.Content = "Đóng";
            }

            // Xử lý Icon và Màu sắc dựa trên Type
            SetupIconAndColor(msg, type);

            return msg.ShowDialog();
        }

        // NEW: Show two-choice options (both primary blue) plus a cancel option "Hủy".
        // Returns the chosen option label (option1 or option2) or null when Hủy/closed.
        public static string? ShowOptions(string message, string title, string option1, string option2, MsgType type = MsgType.Info, bool showCancel = true)
        {
            var msg = new JetMoonMessageBox();
            msg.txtMessage.Text = message;
            msg.txtTitle.Text = title;

            // Set primary option labels (both blue)
            msg.btnOK.Content = option1 ?? "OK";
            msg.btnAlt.Content = option2 ?? "Alt";
            msg.btnAlt.Visibility = Visibility.Visible;

            // Show Cancel as a secondary button labeled "Hủy"
            msg.btnCancel.Content = "Hủy";
            msg.btnCancel.Visibility = Visibility.Visible;

            // Icon/color
            SetupIconAndColor(msg, type);

            // Show dialog and then inspect SelectedOption
            msg.ShowDialog();

            // If user clicked one of the primary buttons, SelectedOption will be that label
            if (string.Equals(msg.SelectedOption, option1)) return option1;
            if (string.Equals(msg.SelectedOption, option2)) return option2;

            // Cancel or closed -> return null
            return null;
        }

        private static void SetupIconAndColor(JetMoonMessageBox msg, MsgType type)
        {
            string pathData = "";
            string colorHex = "#0EA5E9"; // Default Blue
            string bgHex = "#E0F2FE";    // Default Light Blue

            switch (type)
            {
                case MsgType.Info:
                    pathData = "M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z";
                    colorHex = "#0EA5E9"; // Sky Blue
                    bgHex = "#E0F2FE";
                    break;

                case MsgType.Success:
                    pathData = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z";
                    colorHex = "#10B981"; // Emerald Green
                    bgHex = "#ECFDF5";
                    break;

                case MsgType.Warning:
                    pathData = "M12,2L1,21H23M12,6L19.53,19H4.47M11,10V14H13V10M11,16V18H13V16";
                    colorHex = "#F59E0B"; // Amber
                    bgHex = "#FFFBEB";
                    break;

                case MsgType.Error:
                    pathData = "M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z";
                    colorHex = "#EF4444"; // Red
                    bgHex = "#FEE2E2";
                    break;

                case MsgType.Question:
                    pathData = "M12,2C17.52,2 22,6.48 22,12C22,17.52 17.52,22 12,22C6.48,22 2,17.52 2,12C2,6.48 6.48,2 12,2M12,4C7.58,4 4,7.58 4,12C4,16.42 7.58,20 12,20C16.42,20 20,16.42 20,12C20,7.58 16.42,4 12,4M12,15H14V17H12V15M13.5,13.29C13.5,13.29 16,11.5 16,9.5C16,7.74 14.39,6.5 12,6.5C9.89,6.5 8.5,8 8.5,8L9.77,9.45C9.77,9.45 10.66,8.25 12,8.25C13.21,8.25 14,8.87 14,9.5C14,9.97 13.5,10.5 12.8,11.12C12.18,11.67 11.5,12.44 11.5,13.31V13.84H13.5V13.29Z";
                    colorHex = "#6366F1"; // Indigo
                    bgHex = "#EEF2FF";
                    break;
            }

            msg.iconMain.Data = Geometry.Parse(pathData);
            msg.iconMain.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex);
            msg.bdrIcon.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(bgHex);

            // Đổi màu title header theo màu icon luôn cho đồng bộ (Optional)
            // msg.txtTitle.Foreground = msg.iconMain.Fill; 
        }
    }
}