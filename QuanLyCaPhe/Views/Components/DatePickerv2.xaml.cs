using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyCaPhe.Views.Components
{
    public partial class DatePickerv2 : UserControl
    {
        public DatePickerv2()
        {
            InitializeComponent();

            // Gán sự kiện cho các nút và ô nhập
            Prevbtn.Click += Prevbtn_Click;
            Nextbtn.Click += Nextbtn_Click;

            // Xử lý khi người dùng nhập tay và Enter hoặc mất focus
            Monthtb.LostFocus += DateInput_LostFocus;
            Yeartb.LostFocus += DateInput_LostFocus;
            Monthtb.KeyDown += DateInput_KeyDown;
            Yeartb.KeyDown += DateInput_KeyDown;

            // Khởi tạo hiển thị lần đầu
            UpdateUI();
        }

        // 1. DEPENDENCY PROPERTY: SelectedDate
        // Giúp bạn có thể binding: <local:DatePickerv2 SelectedDate="{Binding MyDate}"/>
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime), typeof(DatePickerv2),
                new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

        public DateTime SelectedDate
        {
            get { return (DateTime)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        // Khi giá trị SelectedDate thay đổi (từ Code hoặc từ ViewModel), cập nhật lại giao diện
        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DatePickerv2 control)
            {
                control.UpdateUI();
            }
        }

        // Cập nhật Text cho 2 ô Month và Year
        private void UpdateUI()
        {
            Monthtb.Text = SelectedDate.Month.ToString();
            Yeartb.Text = SelectedDate.Year.ToString();
        }

        // 2. XỬ LÝ SỰ KIỆN NÚT BẤM (PREV / NEXT)
        private void Prevbtn_Click(object sender, RoutedEventArgs e)
        {
            // Giảm 1 tháng
            SelectedDate = SelectedDate.AddMonths(-1);
        }

        private void Nextbtn_Click(object sender, RoutedEventArgs e)
        {
            // Tăng 1 tháng
            SelectedDate = SelectedDate.AddMonths(1);
        }

        // 3. XỬ LÝ NHẬP TAY (Validation)
        private void DateInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Khi nhấn Enter thì ép cập nhật dữ liệu và bỏ focus để kích hoạt LostFocus
                ValidateAndUpdateDate();
                Keyboard.ClearFocus();
            }
        }

        private void DateInput_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateAndUpdateDate();
        }

        private void ValidateAndUpdateDate()
        {
            // Cố gắng parse số từ TextBox
            bool isMonthValid = int.TryParse(Monthtb.Text, out int newMonth);
            bool isYearValid = int.TryParse(Yeartb.Text, out int newYear);

            if (isMonthValid && isYearValid)
            {
                // Kiểm tra logic tháng (1-12) và năm > 0
                if (newMonth < 1) newMonth = 1;
                if (newMonth > 12) newMonth = 12;
                if (newYear < 1) newYear = 2000; // Mặc định nếu nhập sai năm

                try
                {
                    // Tạo ngày mới, giữ nguyên ngày hiện tại (hoặc đưa về ngày 1 để tránh lỗi ngày 31 tháng 2)
                    // Ở đây mình đưa về ngày 1 của tháng mới để an toàn nhất
                    SelectedDate = new DateTime(newYear, newMonth, 1);
                }
                catch
                {
                    // Nếu lỗi ngày tháng không hợp lệ, reset lại hiển thị cũ
                    UpdateUI();
                }
            }
            else
            {
                // Nếu nhập chữ linh tinh, reset lại hiển thị cũ
                UpdateUI();
            }
        }
    }
}