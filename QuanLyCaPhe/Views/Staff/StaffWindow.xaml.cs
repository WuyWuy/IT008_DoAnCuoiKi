using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using QuanLyCaPhe.Views.Login;
using System.Text;
using System.ComponentModel;

namespace QuanLyCaPhe.Views.Staff
{
    public partial class StaffWindow : Window
    {
        public class Drink
        {
            public int Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public int Price { get; init; }
            public override string ToString() => Name;
        }

        public class OrderItem
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;
            public string Price { get; set; } = "0";
            public string Quantity { get; set; } = "0";
            public string Total { get; set; } = "0";
            public int ID
            {
                get => Id;
                set => Id = value;
            }
            public string Mon
            {
                get => Name;
                set => Name = value;
            }

            public string Gia
            {
                get => Price;
                set => Price = value;
            }

            public string SoLuong
            {
                get => Quantity;
                set => Quantity = value;
            }

            public string ThanhTien
            {
                get => Total;
                set => Total = value;
            }
        }

        public List<Drink> Drinks { get; private set; }
        public ObservableCollection<OrderItem> OrderList { get; private set; }
        public ObservableCollection<Table> Tables { get; private set; }
        private bool isUserEditing = false;
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly DispatcherTimer _clockTimer;
        private readonly bool[] _idUsed = new bool[35];
        private int _currentSum = 0;
        private int _selectedHourPrice = 0;

        private static readonly CultureInfo VietCulture = new CultureInfo("vi-VN");
        private const int MaxIds = 35;

        private List<Drink> _allDrinks;

        // references for XAML named controls that do not get generated in this build environment
        // (no manual clock fields here - XAML generates them)

        public StaffWindow()
        {
            InitializeComponent();
            this.Loaded += StaffWindow_Loaded;

            InitializeDrinks();
            _allDrinks = Drinks;
            InitializeTables();

            lbTables.ItemsSource = Tables;

            cbOrder.ItemsSource = Drinks;
            cbOrder.DisplayMemberPath = nameof(Drink.Name);
            cbOrder.SelectedValuePath = nameof(Drink.Id);

            OrderList = new ObservableCollection<OrderItem>();
            dtgdOrder.ItemsSource = OrderList;

            // do not pre-select first item
            cbOrder.SelectedIndex = -1;

            SelectFirstFreeTable();

            UpdateSumDisplay();

            PopulateSwapCombo();

            _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();

            // clock timer
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();

            // set initial clock text
            UpdateClock();
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            //24-hour time HH:mm
            if (tbClockTime != null) tbClockTime.Text = now.ToString("HH:mm");
            // day-month without 'thg'
            if (tbClockDayMonth != null) tbClockDayMonth.Text = now.ToString("dd-MM");
            // year
            if (tbClockYear != null) tbClockYear.Text = now.ToString("yyyy");
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var t in Tables)
            {
                if (t.Countdown > 0)
                {
                    t.Countdown--;
                    if (t.Countdown == 0)
                    {
                        t.Status = "Free";
                    }
                }
            }
        }
        private void SelectFirstFreeTable()
        {
            var firstFree = Tables.FirstOrDefault(t => t.Status == "Free");
            if (firstFree == null) return;

            foreach (var t in Tables.Where(t => t.Status == "Selected")) t.Status = "Free";
            foreach (var t in Tables.Where(t => t.Status == "Selected Busy")) t.Status = "Busy";

            firstFree.Status = "Selected";
            lbTables.SelectedItem = firstFree;
            tbTableNumber.Text = firstFree.Id.ToString();
        }

        private void PopulateSwapCombo()
        {
            if (cbxSwapTable == null) return;

            var source = lbTables.SelectedItem as Table;
            if (source == null)
            {
                cbxSwapTable.ItemsSource = null;
                cbxSwapTable.SelectedIndex = -1;
                return;
            }

            var available = Tables.Where(t => t.Status == "Free" && t.Id != source.Id).ToList();
            cbxSwapTable.ItemsSource = available;
            cbxSwapTable.DisplayMemberPath = nameof(Table.Name);
            cbxSwapTable.SelectedValuePath = nameof(Table.Id);
            cbxSwapTable.SelectedIndex = -1;
        }

        private void ComboBox_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {

            if (cbxSwapTable.SelectedItem == null) return;

            var target = cbxSwapTable.SelectedItem as Table;
            var source = lbTables.SelectedItem as Table;

            if (source == null)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi chuyển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbxSwapTable.SelectedIndex = -1;
                return;
            }

            if (source.Status == "Free" || source.Status == "Selected")
            {
                MessageBox.Show("Bàn đang chọn chưa có khách. Vui lòng chọn bàn có khách để chuyển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbxSwapTable.SelectedIndex = -1;
                return;
            }

            if (target == null) return;

            var confirm = MessageBox.Show($"Bạn có chắc muốn chuyển bàn từ {source.Name} sang {target.Name}?", "Xác nhận chuyển bàn", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes)
            {
                cbxSwapTable.SelectedIndex = -1;
                return;
            }

            var oldStatus = source.Status;
            var oldCountdown = source.Countdown;

            source.Status = "Free";
            source.Countdown = 0;

            target.Status = oldStatus;
            target.Countdown = oldCountdown;

            foreach (var t in Tables.Where(t => t.Id != target.Id))
            {
                if (t.Status == "Selected") t.Status = "Free";
                if (t.Status == "Selected Busy") t.Status = "Busy";
            }

            lbTables.SelectedItem = target;
            tbTableNumber.Text = target.Id.ToString();

            PopulateSwapCombo();
            cbxSwapTable.SelectedIndex = -1;
        }

        private void lbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var table = lbTables.SelectedItem as Table;
            if (table == null) return;

            tbTableNumber.Text = table.Id.ToString();
            PopulateSwapCombo();
        }

        // Sorting handlers for the left-side N / I / D buttons
        private void SortNormal_Click(object sender, RoutedEventArgs e)
        {
            int previouslySelectedId = -1;
            if (lbTables.SelectedItem is Table sel) previouslySelectedId = sel.Id;

            Tables = new ObservableCollection<Table>(Tables.OrderBy(t => t.Id));
            lbTables.ItemsSource = Tables;

            if (previouslySelectedId != -1)
            {
                lbTables.SelectedItem = Tables.FirstOrDefault(t => t.Id == previouslySelectedId);
            }
        }

        private void SortIncrease_Click(object sender, RoutedEventArgs e)
        {
            int previouslySelectedId = -1;
            if (lbTables.SelectedItem is Table sel) previouslySelectedId = sel.Id;

            // sort by countdown (increasing), then by id to keep deterministic order
            Tables = new ObservableCollection<Table>(Tables.OrderBy(t => t.Countdown).ThenBy(t => t.Id));
            lbTables.ItemsSource = Tables;

            if (previouslySelectedId != -1)
            {
                lbTables.SelectedItem = Tables.FirstOrDefault(t => t.Id == previouslySelectedId);
            }
        }

        private void SortDecrease_Click(object sender, RoutedEventArgs e)
        {
            int previouslySelectedId = -1;
            if (lbTables.SelectedItem is Table sel) previouslySelectedId = sel.Id;

            // sort by countdown (decreasing), then by id to keep deterministic order
            Tables = new ObservableCollection<Table>(Tables.OrderByDescending(t => t.Countdown).ThenBy(t => t.Id));
            lbTables.ItemsSource = Tables;

            if (previouslySelectedId != -1)
            {
                lbTables.SelectedItem = Tables.FirstOrDefault(t => t.Id == previouslySelectedId);
            }
        }

        private void Table_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not int id) return;

            var table = Tables.FirstOrDefault(t => t.Id == id);
            if (table == null) return;

            // clear previous selected markers
            foreach (var t in Tables.Where(t => t.Status == "Selected")) t.Status = "Free";
            foreach (var t in Tables.Where(t => t.Status == "Selected Busy")) t.Status = "Busy";

            table.Status = table.Status == "Busy" ? "Selected Busy" : "Selected";

            lbTables.SelectedItem = table;
            tbTableNumber.Text = table.Id.ToString();

            PopulateSwapCombo();
        }
        private void Table_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return;

            if (sender is not Button btn) return;
            if (btn.Tag is not int id) return;

            var table = Tables.FirstOrDefault(t => t.Id == id);
            if (table == null) return;

            if (table.Status == "Busy" || table.Status == "Selected Busy")
            {
                var res = MessageBox.Show($"Bàn {table.Name} ở trạng thái có người. Bạn có muốn xóa trạng thái này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    table.Status = "Free";
                    table.Countdown = 0;

                    lbTables.SelectedItem = table;
                    tbTableNumber.Text = table.Id.ToString();

                    PopulateSwapCombo();
                }

                e.Handled = true;
            }
        }
        private int AllocateId()
        {
            for (int i = 1; i < MaxIds; i++)
            {
                if (!_idUsed[i])
                {
                    _idUsed[i] = true;
                    return i;
                }
            }
            return 0;
        }

        private void ReleaseId(int id)
        {
            if (id >0 && id < MaxIds) _idUsed[id] = false;
        }
        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            iudAmmount.Value = 1;
        }

        public bool IsValid(bool requireHour = true)
        {
            if (string.IsNullOrWhiteSpace(iudAmmount.Text))
            {
                MessageBox.Show("Hãy nhập số lượng", "Order thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (requireHour && _selectedHourPrice <= 0)
            {
                MessageBox.Show("Hãy chọn số giờ sử dụng trước khi order", "Order thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            return true;
        }

        private static int ParseIntFromFormatted(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            var digits = new string(s.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var v) ? v : 0;
        }

        private static string FormatCurrency(int value)
        {
            return value.ToString("N0", VietCulture);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var selectedTable = lbTables.SelectedItem as Table;
            bool requireHour = selectedTable == null || selectedTable.Status == "Free" || selectedTable.Status == "Selected";

            if (!IsValid(requireHour)) return;

            var d = cbOrder.SelectedItem as Drink;
            if (d == null)
            {
                MessageBox.Show( " Vui lòng nhập hoặc chọn đồ order hợp lệ","Order thất bại" ,MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            int amount = iudAmmount.Value ?? 1;
            if (amount <= 0) amount = 1;

            var existed = OrderList.FirstOrDefault(x => x.Name == d.Name);
            if (existed != null)
            {
                int oldAmount = ParseIntFromFormatted(existed.Quantity);
                int newAmount = oldAmount + amount;
                existed.Quantity = FormatCurrency(newAmount);
                existed.Total = FormatCurrency(newAmount * d.Price);
            }
            else
            {
                OrderList.Add(new OrderItem
                {
                    Id = AllocateId(),
                    Name = d.Name,
                    Price = FormatCurrency(d.Price),
                    Quantity = FormatCurrency(amount),
                    Total = FormatCurrency(amount * d.Price)
                });
            }

            OrderList = new ObservableCollection<OrderItem>(OrderList.OrderBy(x => x.Id));
            dtgdOrder.ItemsSource = OrderList;

            _currentSum += d.Price * amount;
            UpdateSumDisplay();

            iudAmmount.Value = 0;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = dtgdOrder.SelectedItem as OrderItem;
            if (item == null)
            {
                MessageBox.Show("Chọn món cần xóa!", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _currentSum -= ParseIntFromFormatted(item.Total);
            ReleaseId(item.Id);
            UpdateSumDisplay();
            OrderList.Remove(item);
        }

        private void dtgdOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = dtgdOrder.SelectedItem as OrderItem;
            if (item == null) return;

            cbOrder.SelectedItem = Drinks.FirstOrDefault(d => d.Name == item.Name);
            iudAmmount.Value = ParseIntFromFormatted(item.Quantity);
        }

        private void iudAmmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (iudAmmount.Value <= 0) iudAmmount.Value = 1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedTable = lbTables.SelectedItem as Table;

            if (selectedTable != null && (selectedTable.Status == "Busy" || selectedTable.Status == "Selected Busy"))
            {
                if (_currentSum == 0 && _selectedHourPrice == 0)
                {
                    MessageBox.Show("Bàn chưa có món hoặc vui lòng chọn số giờ để thanh toán!", "Thanh toán thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var choice = MessageBox.Show("Bạn muốn thanh toán chứ?", "Thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (choice != MessageBoxResult.Yes) return;

                if (_selectedHourPrice > 0)
                {
                    int hours = GetSelectedHoursFromRadioButtons();
                    selectedTable.Countdown += hours * 3600;
                    selectedTable.Status = "Busy";

                    ClearHourSelection();
                    UpdateSumDisplay();
                }

                CompleteOrderCleanup();
                MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK);
                return;
            }

            if (_currentSum == 0)
            {
                MessageBox.Show("Chưa có món để thanh toán!", "Thanh toán thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_selectedHourPrice <= 0)
            {
                MessageBox.Show("Hãy chọn số giờ sử dụng trước khi thanh toán!", "Thanh toán thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var res = MessageBox.Show("Bạn muốn thanh toán chứ?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            if (selectedTable != null)
            {
                int hours = GetSelectedHoursFromRadioButtons();
                selectedTable.Countdown = Math.Max(0, hours) * 3600;
                selectedTable.Status = "Busy";
            }

            CompleteOrderCleanup();
            ClearHourSelection();
            MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK);
        }

        private int GetSelectedHoursFromRadioButtons()
        {
            var checkedRb = spHours.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);
            if (checkedRb != null && int.TryParse(checkedRb.Content?.ToString(), out int hours)) return hours;
            return 0;
        }

        private void CompleteOrderCleanup()
        {
            _currentSum = 0;
            tbSum.Text = "0 VNĐ";
            cbOrder.SelectedIndex = 0;

            if (OrderList != null)
            {
                foreach (var it in OrderList)
                {
                    ReleaseId(it.Id);
                }
                OrderList.Clear();
            }

            dtgdOrder.ItemsSource = OrderList;
        }

        private void ClearHourSelection()
        {
            _selectedHourPrice = 0;
            rbHour1.IsChecked = false;
            rbHour3.IsChecked = false;
            rbHour6.IsChecked = false;
            rbHour12.IsChecked = false;
        }
        private void HourRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && int.TryParse(rb.Tag?.ToString(), out int price))
            {
                _selectedHourPrice = price;
            }
            else
            {
                _selectedHourPrice = 0;
            }

            UpdateSumDisplay();
        }

        private void HourRadio_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                rb.IsChecked = false;
                _selectedHourPrice = 0;
                UpdateSumDisplay();
                e.Handled = true;
            }
        }

        private void UpdateSumDisplay()
        {
            tbSum.Text = $"{FormatCurrency(_currentSum + _selectedHourPrice)} VNĐ";
        }
        public class IntGreaterThanZeroToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is int i && i > 0) return Visibility.Visible;
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // stop background timers
            try { _dispatcherTimer?.Stop(); } catch { }
            try { _clockTimer?.Stop(); } catch { }

            var login = new QuanLyCaPhe.Views.Login.LoginWindow();
            login.Show();
            this.Close();
        }

        private void StaffWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // attach to editable TextBox inside ComboBox
            var textBox = cbOrder.Template.FindName("PART_EditableTextBox", cbOrder) as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                textBox.TextChanged += cbOrder_TextChanged;
            }
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private void cbOrder_TextChanged(object? sender, TextChangedEventArgs e)
        {
            // Lấy TextBox từ sender
    var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null || cbOrder == null) return;

            // 1. LẤY GIÁ TRỊ HIỆN TẠI MÀ NGƯỜI DÙNG ĐANG GÕ
            string currentText = tb.Text;
            int caretIndex = tb.CaretIndex; // Lưu vị trí con trỏ để khôi phục sau

            // 2. GỠ BỎ SỰ KIỆN ĐỂ TRÁNH VÒNG LẶP HOẶC TỰ ĐỘNG CẬP NHẬT
            tb.TextChanged -= cbOrder_TextChanged;

            try
            {
                // 3. XỬ LÝ LỌC DỮ LIỆU (Logic của bạn giữ nguyên)
                var words = currentText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(w => w.Trim())
                                .Where(w => w.Length > 0)
                                .ToArray();

                IEnumerable<Drink> items = _allDrinks ?? new List<Drink>();

                if (words.Length > 0)
                {
                    var normalizedWords = words.Select(w => RemoveDiacritics(w).ToLowerInvariant()).ToArray();
                    items = _allDrinks.Where(d =>
                    {
                        var nameNorm = RemoveDiacritics(d.Name).ToLowerInvariant();
                        return normalizedWords.All(w => nameNorm.IndexOf(w, StringComparison.Ordinal) >= 0);
                    });
                }

                // 4. CẬP NHẬT ITEMSSOURCE
                // Quan trọng: Set lại ItemsSource sẽ làm ComboBox cố gắng chọn lại Item
                cbOrder.ItemsSource = items.ToList();

                // 5. MỞ DROPDOWN ĐỂ HIỆN KẾT QUẢ
                cbOrder.IsDropDownOpen = true;

                // 6. CỰC KỲ QUAN TRỌNG: GÁN LẠI TEXT CŨ
                // Vì bước gán ItemsSource ở trên có thể đã làm thay đổi Text của ComboBox,
                // ta phải ép nó quay về cái text người dùng đang gõ.
                cbOrder.Text = currentText;

                // Hoặc gán trực tiếp vào TextBox nếu binding của ComboBox chưa cập nhật kịp
                tb.Text = currentText;

                // 7. KHÔI PHỤC VỊ TRÍ CON TRỎ
                tb.CaretIndex = caretIndex;
            }
            finally
            {
                // 8. GẮN LẠI SỰ KIỆN ĐỂ LẦN GÕ TIẾP THEO HOẠT ĐỘNG
                tb.TextChanged += cbOrder_TextChanged;
            }
        }

        private void InitializeDrinks()
        {
            Drinks = new List<Drink>
            {
                new() { Id =1, Name = "Cà phê đen", Price =15000 },
                new() { Id =2, Name = "Cà phê sữa", Price =17000 },
                new() { Id =3, Name = "Cà phê muối", Price =20000 },
                new() { Id =4, Name = "Soda dâu", Price =23000 },
                new() { Id =5, Name = "Soda việt quất", Price =25000 },
                new() { Id =6, Name = "Soda chanh", Price =20000 },
                new() { Id =7, Name = "Nước ép cam", Price =18000 },
                new() { Id =8, Name = "Nước ép dưa hấu", Price =20000 },
                new() { Id =9, Name = "Nước ép chanh", Price =15000 },
                new() { Id =10, Name = "Matcha đá xay", Price =20000 },
                new() { Id =11, Name = "Cookie đá xay", Price =25000 },
                new() { Id =12, Name = "Matcha latte", Price =23000 }
            };
        }

        private void InitializeTables()
        {
            Tables = new ObservableCollection<Table>(
                Enumerable.Range(1, 12).Select(i => new Table { Id = i, Name = $"Bàn {i}" })
            );
        }
    }
}