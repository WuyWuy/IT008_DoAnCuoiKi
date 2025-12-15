using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanLyCaPhe.Views.SharedPage
{
    public partial class SchedulePage : Page
    {
        // Cấu hình thời gian
        private const int StartHour = 6;
        private const int EndHour = 22;
        private const int StepMinutes = 30;

        // Quản lý thời gian hiện tại
        private DateTime _currentMonthDisplay;
        private DateTime _selectedDate; // Ngày được chọn khi click
        private TimeSpan _selectedStartTime; // Giờ bắt đầu khi click

        // Biến lưu trữ danh sách các ca đã thêm để kiểm tra va chạm
        // Key: "DayIndex_SlotIndex", Value: Số lượng ca đang có ở đó
        private Dictionary<string, int> _slotOccupancy = new Dictionary<string, int>();

        public SchedulePage()
        {
            InitializeComponent();
            _currentMonthDisplay = DateTime.Now; // Mặc định tháng hiện tại
            InitComboBoxes();
            RenderSchedule();
        }

        private void InitComboBoxes()
        {
            cbEndHour.Items.Clear();
            cbEndMinute.Items.Clear();
            for (int i = StartHour; i <= EndHour; i++) cbEndHour.Items.Add(i);
            cbEndMinute.Items.Add(0);
            cbEndMinute.Items.Add(30);
        }

        // --- NAVIGATION THÁNG ---
        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonthDisplay = _currentMonthDisplay.AddMonths(-1);
            RenderSchedule();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonthDisplay = _currentMonthDisplay.AddMonths(1);
            RenderSchedule();
        }

        // --- CORE: VẼ LỊCH THEO THÁNG ---
        private void RenderSchedule()
        {
            // Cập nhật text hiển thị tháng
            txtCurrentMonth.Text = $"Tháng {_currentMonthDisplay.Month}, {_currentMonthDisplay.Year}";

            ScheduleGrid.Children.Clear();
            ScheduleGrid.RowDefinitions.Clear();
            ScheduleGrid.ColumnDefinitions.Clear();

            int daysInMonth = DateTime.DaysInMonth(_currentMonthDisplay.Year, _currentMonthDisplay.Month);
            int totalSlots = (EndHour - StartHour) * (60 / StepMinutes);

            // 1. TẠO CỘT
            // Cột Ngày
            ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            // Các cột Slot (30p)
            for (int i = 0; i < totalSlots; i++)
            {
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            }

            // 2. TẠO HÀNG
            ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) }); // Header
            for (int i = 1; i <= daysInMonth; i++)
            {
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            }

            // 3. VẼ HEADER GIỜ (LOGIC MỚI - CLEAN CODE)
            // Chỉ chạy i < totalSlots (Không vẽ mốc cuối cùng 22h)
            for (int i = 0; i < totalSlots; i += 2)
            {
                var time = new TimeSpan(StartHour, 0, 0).Add(TimeSpan.FromMinutes(i * StepMinutes));

                Border headerBorder = new Border
                {
                    Style = (Style)FindResource("HeaderBorderStyle"),
                    Width = 120, // Rộng 120px (chiếm trọn 1 tiếng)

                    // Căn Trái: Chữ bắt đầu ngay tại vạch kẻ
                    HorizontalAlignment = HorizontalAlignment.Left,

                    // Kéo lùi nhẹ 15px để tâm chữ "06" nằm giữa vạch kẻ cho đẹp mắt (giống Google Calendar)
                    // Nếu bạn thích nó nằm hẳn bên trong ô thì sửa thành (5,0,0,0)
                    Margin = new Thickness(0, 0, 0, 0)
                };

                TextBlock txtTime = new TextBlock
                {
                    Text = time.ToString(@"hh\:mm"),
                    Style = (Style)FindResource("HeaderTextStyle"),
                    // Vì Border đã căn Left rồi, TextBlock chỉ cần Center trong Border hoặc Left tùy ý
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                headerBorder.Child = txtTime;

                // ĐỊNH VỊ
                Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, i + 1);

                // Luôn luôn Span 2 cột (Vì ta đã bỏ mốc cuối cùng, nên các mốc còn lại luôn đủ chỗ chứa 1 tiếng)
                Grid.SetColumnSpan(headerBorder, 2);

                // VẼ VẠCH KẺ DỌC
                Border guideLine = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                    BorderThickness = new Thickness(1, 0, 0, 0),
                    Opacity = 0.5
                };
                Grid.SetRow(guideLine, 1);
                Grid.SetRowSpan(guideLine, daysInMonth);
                Grid.SetColumn(guideLine, i + 1);

                ScheduleGrid.Children.Add(guideLine);
                ScheduleGrid.Children.Add(headerBorder);
            }

            // 4. VẼ NGÀY VÀ CELL (Giữ nguyên logic cũ)
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(_currentMonthDisplay.Year, _currentMonthDisplay.Month, day);

                // Header Ngày
                Border dayHeader = new Border { Style = (Style)FindResource("HeaderBorderStyle"), Padding = new Thickness(10, 0, 0, 0) };
                StackPanel spDay = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

                Brush primaryColor = (Brush)TryFindResource("PrimaryColor") ?? Brushes.Black;

                TextBlock txtDayNum = new TextBlock
                {
                    Text = day.ToString("00"),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = primaryColor
                };
                TextBlock txtDayName = new TextBlock { Text = GetVietnameseDayName(date.DayOfWeek), FontSize = 11, Foreground = Brushes.Gray };

                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    txtDayNum.Foreground = Brushes.Red;
                    txtDayName.Foreground = Brushes.Red;
                }

                spDay.Children.Add(txtDayNum);
                spDay.Children.Add(txtDayName);
                dayHeader.Child = spDay;

                Grid.SetRow(dayHeader, day);
                Grid.SetColumn(dayHeader, 0);
                ScheduleGrid.Children.Add(dayHeader);

                // Cells
                for (int c = 0; c < totalSlots; c++)
                {
                    var currentTime = new TimeSpan(StartHour, 0, 0).Add(TimeSpan.FromMinutes(c * StepMinutes));
                    Border cell = new Border { Style = (Style)FindResource("GridCellStyle") };
                    cell.Tag = new { Date = date, Time = currentTime, ColIndex = c + 1 };
                    cell.MouseLeftButtonDown += Cell_Click;
                    Grid.SetRow(cell, day);
                    Grid.SetColumn(cell, c + 1);
                    ScheduleGrid.Children.Add(cell);
                }
            }
        }

        // Helper: Tên thứ tiếng Việt
        private string GetVietnameseDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Thứ 2";
                case DayOfWeek.Tuesday: return "Thứ 3";
                case DayOfWeek.Wednesday: return "Thứ 4";
                case DayOfWeek.Thursday: return "Thứ 5";
                case DayOfWeek.Friday: return "Thứ 6";
                case DayOfWeek.Saturday: return "Thứ 7";
                case DayOfWeek.Sunday: return "CN";
                default: return "";
            }
        }

        // --- SỰ KIỆN CLICK VÀO Ô ---
        private void Cell_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            dynamic data = border.Tag;
            _selectedDate = data.Date;
            _selectedStartTime = data.Time;

            // Fill dữ liệu vào Popup
            txtDate.Text = _selectedDate.ToString("dd/MM/yyyy");
            txtStartTime.Text = _selectedStartTime.ToString(@"hh\:mm");
            txtName.Text = "";
            txtError.Text = "";

            // Auto chọn giờ kết thúc (+1 tiếng)
            var endTime = _selectedStartTime.Add(TimeSpan.FromHours(1));
            if (endTime > new TimeSpan(EndHour, 0, 0)) endTime = new TimeSpan(EndHour, 0, 0);

            cbEndHour.SelectedItem = endTime.Hours;
            cbEndMinute.SelectedItem = endTime.Minutes;

            PopupOverlay.Visibility = Visibility.Visible;
            txtName.Focus();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { txtError.Text = "Nhập tên nhân viên!"; return; }
            if (cbEndHour.SelectedItem == null) return;

            int endH = (int)cbEndHour.SelectedItem;
            int endM = (int)cbEndMinute.SelectedItem;
            TimeSpan endTime = new TimeSpan(endH, endM, 0);

            if (endTime <= _selectedStartTime) { txtError.Text = "Giờ kết thúc không hợp lệ."; return; }

            TimeSpan duration = endTime - _selectedStartTime;
            if (duration.TotalMinutes % 30 != 0) { txtError.Text = "Thời gian phải chẵn 30p."; return; }

            // Vẽ Shift lên Grid
            AddShiftToGrid(txtName.Text, _selectedDate, _selectedStartTime, duration);
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        // --- VẼ KHỐI CA LÀM ---
        private void AddShiftToGrid(string name, DateTime date, TimeSpan start, TimeSpan duration)
        {
            // Tìm dòng (Row) tương ứng với ngày
            // Vì Row 0 là Header nên Row index = Day
            int rowIndex = date.Day;

            // Tìm cột (Column)
            int startSlotIndex = (int)((start.TotalMinutes - (StartHour * 60)) / StepMinutes);
            int spanCount = (int)(duration.TotalMinutes / StepMinutes);

            Border shiftBlock = new Border
            {
                Style = (Style)FindResource("ShiftBlockStyle"),
                ToolTip = $"{name} ({start:hh\\:mm} - {start.Add(duration):hh\\:mm})"
            };

            TextBlock txtContent = new TextBlock
            {
                Text = $"{name}\n{start:hh\\:mm}-{start.Add(duration):hh\\:mm}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(67, 56, 202)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            shiftBlock.Child = txtContent;

            Grid.SetRow(shiftBlock, rowIndex);
            Grid.SetColumn(shiftBlock, startSlotIndex + 1);
            Grid.SetColumnSpan(shiftBlock, spanCount);

            ScheduleGrid.Children.Add(shiftBlock);
        }
    }
}