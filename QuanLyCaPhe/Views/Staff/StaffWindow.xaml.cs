
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
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

// Import các Views và DAO
using QuanLyCaPhe.Views.Login;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Views.Admin;
using QuanLyCaPhe.Views.Components; // <-- QUAN TRỌNG: Để dùng JetMoonMessageBox

using PayOS;
using PayOS.Models;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace QuanLyCaPhe.Views.Staff
{
    public partial class StaffWindow : Window
    {
        // --- CÁC CLASS MODEL NỘI BỘ ---
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
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Price { get; set; } = "0";
            public string Quantity { get; set; } = "0";
            public string Total { get; set; } = "0";

            // Properties cho DataGrid Binding
            public int ID { get => Id; set => Id = value; }
            public string Mon { get => Name; set => Name = value; }
            public string Gia { get => Price; set => Price = value; }
            public string SoLuong { get => Quantity; set => Quantity = value; }
            public string ThanhTien { get => Total; set => Total = value; }
        }

        private record TableState(int Id, string Name, string Status, DateTime? EndTimeUtc);

        // --- CÁC BIẾN TOÀN CỤC ---
        public List<Drink> Drinks { get; private set; }
        public ObservableCollection<OrderItem> OrderList { get; private set; }
        public ObservableCollection<Table> Tables { get; private set; }

        private readonly DispatcherTimer _dispatcherTimer;
        private readonly DispatcherTimer _clockTimer;
        private readonly bool[] _idUsed = new bool[35];
        private int _currentSum = 0;
        private int _selectedHourPrice = 0;

        private static readonly CultureInfo VietCulture = new CultureInfo("vi-VN");
        private const int MaxIds = 35;

        private List<Drink> _allDrinks;
        private readonly string _tablesStatePath;
        private bool _isLogout = false;

        // Cờ chặn sự kiện TextChanged
        private bool _suspendCbOrderTextChanged = false;



        // --- CONSTRUCTOR ---
        public StaffWindow()
        {
            InitializeComponent();
            this.Loaded += StaffWindow_Loaded;
            this.Closing += StaffWindow_Closing;

            // Đường dẫn lưu trạng thái bàn (AppData)
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "QuanLyCaPhe");
            Directory.CreateDirectory(folder);
            _tablesStatePath = Path.Combine(folder, "tables.json");

            // Khởi tạo dữ liệu
            InitializeDrinks();
            _allDrinks = Drinks;
            InitializeTables();

            lbTables.ItemsSource = Tables;

            // Setup ComboBox Order
            cbOrder.ItemsSource = Drinks;
            cbOrder.DisplayMemberPath = nameof(Drink.Name);
            cbOrder.SelectedValuePath = nameof(Drink.Id);
            cbOrder.SelectedIndex = -1;

            // Setup DataGrid
            OrderList = new ObservableCollection<OrderItem>();
            dtgdOrder.ItemsSource = OrderList;

            // Chọn bàn trống đầu tiên
            SelectFirstFreeTable();

            UpdateSumDisplay();
            PopulateSwapCombo();

            // Timer đếm ngược (Mỗi 1s)
            _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();

            // Timer đồng hồ hệ thống (Mỗi 1s)
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();
            UpdateClock();

            // Setup Sort ComboBox
            cbSort.Items.Add("Mặc định A-Z");
            cbSort.Items.Add("Tăng dần - thời gian");
            cbSort.Items.Add("Giảm dần - thời gian");
            cbSort.SelectedIndex = 0;
        }

        // --- CÁC HÀM XỬ LÝ SỰ KIỆN WINDOW ---

        private void StaffWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Gán sự kiện TextChanged cho TextBox bên trong ComboBox (để tìm kiếm)
            var textBox = cbOrder.Template.FindName("PART_EditableTextBox", cbOrder) as TextBox;
            if (textBox != null)
            {
                textBox.TextChanged += cbOrder_TextChanged;
            }
        }

        private void StaffWindow_Closing(object? sender, CancelEventArgs e)
        {
            try { _dispatcherTimer?.Stop(); } catch { }
            try { _clockTimer?.Stop(); } catch { }
            SaveTablesState();

            if (!_isLogout)
            {
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {

            var res = JetMoonMessageBox.Show(
                    $"Bạn có chắc chắn muốn đăng xuất không ?",
                    "Xác nhận đắng xuất",
                    MsgType.Question,
                    true);

            if (res == false) return;

            _isLogout = true;

            try { _dispatcherTimer?.Stop(); } catch { }
            try { _clockTimer?.Stop(); } catch { }

            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        // --- LOGIC ĐỒNG HỒ & TIMER ---

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            if (tbClockTime != null) tbClockTime.Text = now.ToString("HH:mm");
            if (tbClockDayMonth != null) tbClockDayMonth.Text = now.ToString("dd-MM");
            if (tbClockYear != null) tbClockYear.Text = now.ToString("yyyy");
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var t in Tables)
            {
                if (t.EndTimeUtc.HasValue)
                {
                    var remaining = (int)Math.Max(0, (t.EndTimeUtc.Value - DateTime.UtcNow).TotalSeconds);
                    t.Countdown = remaining;
                    if (remaining == 0)
                    {
                        t.EndTimeUtc = null;
                        t.Status = "Free";
                    }
                }
                else
                {
                    if (t.Countdown > 0)
                    {
                        t.Countdown = Math.Max(0, t.Countdown - 1);
                        if (t.Countdown == 0) t.Status = "Free";
                    }
                }
            }
        }

        // --- QUẢN LÝ BÀN (TABLES) ---

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

        private void lbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var table = lbTables.SelectedItem as Table;
            if (table == null) return;

            // Reset trạng thái chọn cũ
            foreach (var t in Tables.Where(t => t.Status == "Selected")) t.Status = "Free";
            foreach (var t in Tables.Where(t => t.Status == "Selected Busy")) t.Status = "Busy";

            // Set trạng thái chọn mới
            table.Status = table.Status == "Busy" ? "Selected Busy" : "Selected";

            tbTableNumber.Text = table.Id.ToString();
            PopulateSwapCombo();
        }

        private void lbTables_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            lbTables_SelectionChanged(sender, e);
        }

        private void Table_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not int id) return;

            var table = Tables.FirstOrDefault(t => t.Id == id);
            if (table == null) return;

            foreach (var t in Tables.Where(t => t.Status == "Selected")) t.Status = "Free";
            foreach (var t in Tables.Where(t => t.Status == "Selected Busy")) t.Status = "Busy";

            table.Status = table.Status == "Busy" ? "Selected Busy" : "Selected";

            lbTables.SelectedItem = table;
            tbTableNumber.Text = table.Id.ToString();

            PopulateSwapCombo();
        }

        private void Table_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return; // Chỉ xử lý Double Click

            if (sender is not Button btn) return;
            if (btn.Tag is not int id) return;

            var table = Tables.FirstOrDefault(t => t.Id == id);
            if (table == null) return;

            if (table.Status == "Busy" || table.Status == "Selected Busy")
            {
                // [FIXED] Dùng JetMoonMessageBox
                var res = JetMoonMessageBox.Show(
                    $"{table.Name} đang có người. Bạn có muốn xóa trạng thái này không?",
                    "Xác nhận Reset Bàn",
                    MsgType.Question,
                    true);

                if (res == true)
                {
                    table.Status = "Free";
                    table.EndTimeUtc = null;
                    table.Countdown = 0;

                    lbTables.SelectedItem = table;
                    tbTableNumber.Text = table.Id.ToString();
                    PopulateSwapCombo();
                }

                e.Handled = true;
            }
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
                // [FIXED] Dùng JetMoonMessageBox
                JetMoonMessageBox.Show("Vui lòng chọn bàn cần chuyển trước.", "Thông báo", MsgType.Warning);
                cbxSwapTable.SelectedIndex = -1;
                return;
            }

            if (source.Status == "Free" || source.Status == "Selected")
            {
                // [FIXED] Dùng JetMoonMessageBox
                JetMoonMessageBox.Show("Bàn đang chọn chưa có khách. Vui lòng chọn bàn ĐANG CÓ KHÁCH để chuyển.", "Thông báo", MsgType.Warning);
                cbxSwapTable.SelectedIndex = -1;
                return;
            }

            if (target == null) return;

            // [FIXED] Dùng JetMoonMessageBox
            var confirm = JetMoonMessageBox.Show(
                $"Bạn có chắc muốn chuyển khách từ {source.Name} sang {target.Name} không?",
                "Xác nhận chuyển bàn",
                MsgType.Question,
                true);

            if (confirm != true)
            {
                cbxSwapTable.SelectedIndex = -1;
                return;
            }

            // Thực hiện chuyển bàn
            var oldStatus = source.Status;
            var oldEnd = source.EndTimeUtc;

            source.Status = "Free";
            source.EndTimeUtc = null;
            source.Countdown = 0;

            target.Status = oldStatus;
            target.EndTimeUtc = oldEnd;
            if (oldEnd.HasValue)
            {
                target.Countdown = (int)Math.Max(0, (oldEnd.Value - DateTime.UtcNow).TotalSeconds);
            }
            else
            {
                target.Countdown = 0;
            }

            // Update UI các bàn khác
            foreach (var t in Tables.Where(t => t.Id != target.Id))
            {
                if (t.Status == "Selected") t.Status = "Free";
                if (t.Status == "Selected Busy") t.Status = "Busy";
            }

            lbTables.SelectedItem = target;
            tbTableNumber.Text = target.Id.ToString();

            PopulateSwapCombo();
            cbxSwapTable.SelectedIndex = -1;

            JetMoonMessageBox.Show("Chuyển bàn thành công!", "Hoàn tất", MsgType.Success);
        }


        // --- LOGIC SẮP XẾP BÀN (SORT) ---

        private void cbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSort == null) return;
            int idx = cbSort.SelectedIndex;
            switch (idx)
            {
                case 0: SortNormal_Click(null, null); break;
                case 1: SortIncrease_Click(null, null); break;
                case 2: SortDecrease_Click(null, null); break;
                default: SortNormal_Click(null, null); break;
            }
        }

        private void SortNormal_Click(object? sender, RoutedEventArgs? e)
        {
            int prevId = -1;
            if (lbTables.SelectedItem is Table sel) prevId = sel.Id;

            Tables = new ObservableCollection<Table>(Tables.OrderBy(t => t.Id));
            lbTables.ItemsSource = Tables;

            if (prevId != -1) lbTables.SelectedItem = Tables.FirstOrDefault(t => t.Id == prevId);
        }

        private void SortIncrease_Click(object? sender, RoutedEventArgs? e)
        {
            int prevId = -1;
            if (lbTables.SelectedItem is Table sel) prevId = sel.Id;

            Tables = new ObservableCollection<Table>(Tables.OrderBy(t => t.Countdown).ThenBy(t => t.Id));
            lbTables.ItemsSource = Tables;

            if (prevId != -1) lbTables.SelectedItem = Tables.FirstOrDefault(t => t.Id == prevId);
        }

        private void SortDecrease_Click(object? sender, RoutedEventArgs? e)
        {
            int prevId = -1;
            if (lbTables.SelectedItem is Table sel) prevId = sel.Id;

            Tables = new ObservableCollection<Table>(Tables.OrderByDescending(t => t.Countdown).ThenBy(t => t.Id));
            lbTables.ItemsSource = Tables;

            if (prevId != -1) lbTables.SelectedItem = Tables.FirstOrDefault(t => t.Id == prevId);
        }

        // --- LOGIC GỌI MÓN (ORDER) ---

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            iudAmmount.Value = 1;
            var d = cbOrder.SelectedItem as Drink;
            if (d == null)
            {
                btnAdd.IsEnabled = false;
                return;
            }
            if (!CanOrderProduct(d.Id, 1, out var msg))
            {
                btnAdd.IsEnabled = false;
                btnAdd.ToolTip = msg;
            }
            else
            {
                btnAdd.IsEnabled = true;
                btnAdd.ToolTip = null;
            }
        }

        public bool IsValid(bool requireHour = true)
        {
            if (string.IsNullOrWhiteSpace(iudAmmount.Value.ToString()) || iudAmmount.Value <= 0)
            {
                // [FIXED]
                JetMoonMessageBox.Show("Vui lòng nhập số lượng hợp lệ!", "Lỗi nhập liệu", MsgType.Error);
                return false;
            }

            if (requireHour && _selectedHourPrice <= 0)
            {
                // [FIXED]
                JetMoonMessageBox.Show("Vui lòng chọn số giờ (gói giờ) trước khi gọi món!", "Quy trình Order", MsgType.Warning);
                return false;
            }

            return true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var selectedTable = lbTables.SelectedItem as Table;
            // Nếu bàn đang trống thì bắt buộc phải chọn giờ trước
            bool requireHour = selectedTable == null || selectedTable.Status == "Free" || selectedTable.Status == "Selected";

            if (!IsValid(requireHour)) return;

            var d = cbOrder.SelectedItem as Drink;
            if (d == null) return;

            int amount = iudAmmount.Value;
            if (amount <= 0) amount = 1;

            // Kiểm tra tồn kho
            var existed = OrderList.FirstOrDefault(x => x.Name == d.Name);
            int existingQty = existed != null ? ParseIntFromFormatted(existed.Quantity) : 0;
            int totalRequested = existingQty + amount;

            if (!CanOrderProduct(d.Id, totalRequested, out var failMsg))
            {
                JetMoonMessageBox.Show(failMsg, "Thiếu nguyên liệu", MsgType.Warning);
                return;
            }

            // Thêm vào danh sách Order
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
                    ProductId = d.Id,
                    Name = d.Name,
                    Price = FormatCurrency(d.Price),
                    Quantity = FormatCurrency(amount),
                    Total = FormatCurrency(amount * d.Price)
                });
            }

            // Refresh UI
            OrderList = new ObservableCollection<OrderItem>(OrderList.OrderBy(x => x.Id));
            dtgdOrder.ItemsSource = OrderList;

            _currentSum += d.Price * amount;
            UpdateSumDisplay();

            iudAmmount.Value = 1; // Reset về 1

            // [ĐÃ SỬA LỖI] Ghi log hoạt động đúng cú pháp
            QuanLyCaPhe.Services.GlobalService.RecordActivity(
                "Order",
                $"Order mới: {selectedTable?.Name ?? "Khách mang về"}",
                $"{d.Name} - Số lượng: {amount}" // Dùng biến d.Name và amount có sẵn
            );
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = dtgdOrder.SelectedItem as OrderItem;
            if (item == null)
            {
                // [FIXED]
                JetMoonMessageBox.Show("Vui lòng chọn món cần xóa trong danh sách!", "Thao tác lỗi", MsgType.Error);
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

        private void iudAmmount_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (iudAmmount.Value <= 0) iudAmmount.Value = 1;
        }

        // --- LOGIC THANH TOÁN (PURCHASE) ---
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedTable = lbTables.SelectedItem as Table;
            if (selectedTable == null) return;

            // Require both time and at least one drink before allowing payment
            int hours = GetSelectedHoursFromRadioButtons();
            if (hours <= 0)
            {
                JetMoonMessageBox.Show("Vui lòng chọn số giờ (gói giờ) trước khi thanh toán!", "Quy trình Order", MsgType.Warning, true);
                return;
            }

            if (OrderList == null || OrderList.Count == 0 || _currentSum <= 0)
            {
                JetMoonMessageBox.Show("Vui lòng chọn món trước khi thanh toán!", "Quy trình Order", MsgType.Warning, true);
                return;
            }

            // Ask user for payment method (modal) using ShowOptions (returns chosen label or null)
            string? chosenMethod = null;
            try
            {
                chosenMethod = JetMoonMessageBox.ShowOptions(
                    $"Chọn phương thức thanh toán",
                    "Phương thức thanh toán",
                    "Tiền mặt",
                    "Chuyển khoản",
                    MsgType.Info,
                    true);
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(chosenMethod))
            {
                // user cancelled
                return;
            }

            try
            {
                // Compute intended checkout time but DO NOT apply it to the UI/table yet.
                DateTime? intendedEndUtc = null;
                if (hours > 0)
                {
                    var baseTime = selectedTable.EndTimeUtc ?? DateTime.UtcNow;
                    intendedEndUtc = baseTime.AddHours(hours);
                }

                // Check inventory availability before showing any further payment UI.
                if (_currentSum > 0)
                {
                    if (!CanFulfillOrderList(out var msg))
                    {
                        JetMoonMessageBox.Show(msg, "Kho không đủ nguyên liệu", MsgType.Warning);
                        return;
                    }
                }

                // If transfer chosen, show VietQR (modal). Do NOT touch the table UI yet.
                if (string.Equals(chosenMethod, "Chuyển khoản", StringComparison.OrdinalIgnoreCase))
                {
                    decimal totalAmount = _currentSum + _selectedHourPrice;
                    int amountInt = Convert.ToInt32(totalAmount);
                    int nextBillNumber = 1;
                    try
                    {
                        var bills = BillDAO.Instance.GetListBills();
                        if (bills != null) nextBillNumber = bills.Count + 1;
                    }
                    catch { nextBillNumber = 1; }

                    string addInfo = $"Thanh toán hóa đơn {nextBillNumber}";
                    string accountName = "Nguyễn Huy";

                    string quickLink = "https://img.vietqr.io/image/970415-107875046252-compact2.png";
                    string url = BuildVietQrUrl(quickLink, amountInt, addInfo, accountName);

                    bool confirmed = ShowVietQrDialog(url, amountInt, addInfo, accountName);

                    if (!confirmed)
                    {
                        // User cancelled. Do not modify table UI, do not deduct inventory.
                        return;
                    }

                    // User confirmed payment (clicked "Đã thanh toán") — proceed to save bill.
                }

                // For cash or after transfer confirmation:
                // Save bill and details (pass the intended checkout time). Inventory will be deducted after successful save.
                int billId = SaveBillAndDetails(selectedTable, intendedEndUtc, null, hours > 0 ? hours : (int?)null, chosenMethod);
                if (billId <= 0)
                {
                    JetMoonMessageBox.Show("Lỗi khi lưu hóa đơn.", "Lỗi", MsgType.Error);
                    return;
                }

                // Deduct inventory now that bill is recorded.
                if (_currentSum > 0)
                {
                    DeductIngredientsForOrderList();
                }

                // Now apply the UI change (table becomes Busy and timer starts)
                if (intendedEndUtc.HasValue)
                {
                    selectedTable.EndTimeUtc = intendedEndUtc;
                    selectedTable.Countdown = (int)Math.Max(0, (intendedEndUtc.Value - DateTime.UtcNow).TotalSeconds);
                }
                else
                {
                    selectedTable.EndTimeUtc = null;
                    selectedTable.Countdown = 0;
                }
                selectedTable.Status = "Busy";

                // Clear UI and reset order
                CompleteOrderCleanup();

                // Clear radio button hour selection after successful purchase
                ClearHourSelection();

                JetMoonMessageBox.Show("Thanh toán thành công!", "Hoàn tất", MsgType.Success);
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi khi xử lý thanh toán: " + ex.Message, "Lỗi", MsgType.Error);
            }
        }

        // --- CÁC HÀM HỖ TRỢ LOGIC (HELPER) ---
        private void DeductIngredientsForOrderList()
        {
            if (OrderList == null || OrderList.Count == 0) return;
            var needed = new Dictionary<int, double>();
            foreach (var it in OrderList)
            {
                int qty = ParseIntFromFormatted(it.Quantity);
                if (qty <= 0) continue;
                var recs = RecipeDAO.Instance.GetListRecipeByProID(it.ProductId);
                foreach (var r in recs)
                {
                    if (!needed.ContainsKey(r.IngId)) needed[r.IngId] = 0;
                    needed[r.IngId] += r.Amount * qty;
                }
            }
            foreach (var kv in needed)
            {
                IngredientDAO.Instance.UpdateQuantity(kv.Key, -kv.Value);
            }
        }

        // --- CÁC HÀM HỖ TRỢ LOGIC (HELPER) ---

        private bool CanOrderProduct(int productId, int qty, out string message)
        {
            message = string.Empty;
            var recipes = RecipeDAO.Instance.GetListRecipeByProID(productId);
            if (recipes == null || recipes.Count == 0) return true;

            var ingredients = IngredientDAO.Instance.GetListIngredient();
            foreach (var r in recipes)
            {
                var ing = ingredients.FirstOrDefault(i => i.Id == r.IngId);
                if (ing == null)
                {
                    message = $"Không tìm thấy nguyên liệu ID={r.IngId}.";
                    return false;
                }
                var required = r.Amount * qty;
                if (ing.Quantity < required)
                {
                    message = $"Không đủ {ing.IngName}. Cần {required} {ing.Unit}, kho chỉ còn {ing.Quantity}.";
                    return false;
                }
            }
            return true;
        }

        private bool CanFulfillOrderList(out string message)
        {
            message = string.Empty;
            if (OrderList == null || OrderList.Count == 0) return true;

            var needed = new Dictionary<int, double>();
            foreach (var it in OrderList)
            {
                int qty = ParseIntFromFormatted(it.Quantity);
                if (qty <= 0) continue;
                var recs = RecipeDAO.Instance.GetListRecipeByProID(it.ProductId);
                foreach (var r in recs)
                {
                    if (!needed.ContainsKey(r.IngId)) needed[r.IngId] = 0;
                    needed[r.IngId] += r.Amount * qty;
                }
            }

            var ingredients = IngredientDAO.Instance.GetListIngredient();
            foreach (var kv in needed)
            {
                var ing = ingredients.FirstOrDefault(i => i.Id == kv.Key);
                if (ing == null)
                {
                    message = $"Không tìm thấy nguyên liệu ID={kv.Key}.";
                    return false;
                }
                if (ing.Quantity < kv.Value)
                {
                    message = $"Không đủ {ing.IngName}. Cần {kv.Value} {ing.Unit}, kho chỉ còn {ing.Quantity}.";
                    return false;
                }
            }
            return true;
        }

        private int SaveBillAndDetails(Table table, DateTime? checkOutUtc, int? userId, int? timeUsedHours, string? paymentMethod)
        {
            try
            {
                if (userId == null)
                {
                    if (Application.Current != null && Application.Current.Properties.Contains("CurrentUserId"))
                    {
                        try { userId = (int?)Application.Current.Properties["CurrentUserId"]; } catch { }
                    }
                }

                DateTime checkIn = DateTime.Now;
                DateTime? checkOut = checkOutUtc?.ToLocalTime();
                int status = 1; // 1 = Paid
                int discount = 0;
                decimal totalPrice = _currentSum + _selectedHourPrice;

                // call updated DAO InsertBill that stores paymentMethod and timeUsedHours
                int billId = BillDAO.Instance.InsertBill(checkIn, checkOut, status, discount, totalPrice, table.Id, userId, paymentMethod, timeUsedHours);
                if (billId <= 0) return -1;

                foreach (var it in OrderList)
                {
                    int proId = it.ProductId;
                    int count = ParseIntFromFormatted(it.Quantity);
                    if (count <= 0) continue;

                    string query = "INSERT INTO BillInfos (BillId, ProId, Count) VALUES (@billId, @proId, @count)";
                    DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@billId", billId),
                new SqlParameter("@proId", proId),
                new SqlParameter("@count", count)
            });
                }
                return billId;
            }
            catch
            {
                return -1;
            }
        }

        // --- CÁC HÀM UTILS KHÁC ---

        private int GetSelectedHoursFromRadioButtons()
        {
            var checkedRb = spHours.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);
            if (checkedRb != null && int.TryParse(checkedRb.Content?.ToString(), out int hours)) return hours;
            return 0;
        }
        private void CompleteOrderCleanup()
        {
            // Pause TextChanged handler while we reset controls
            _suspendCbOrderTextChanged = true;

            try
            {
                _currentSum = 0;
                tbSum.Text = "0 VNĐ";

                // Reset ComboBox: restore the full source, clear selection and editable text,
                // and ensure the drop-down is closed.
                try
                {
                    // Restore full item source so subsequent typing/search shows all items
                    cbOrder.ItemsSource = _allDrinks;
                }
                catch { }

                cbOrder.SelectedItem = null;
                cbOrder.SelectedIndex = -1;
                cbOrder.Text = string.Empty;
                cbOrder.IsDropDownOpen = false;
                cbOrder.ToolTip = null;

                // Also clear the internal editable TextBox if present in the control template
                try
                {
                    var tbEditable = cbOrder.Template.FindName("PART_EditableTextBox", cbOrder) as System.Windows.Controls.TextBox;
                    if (tbEditable != null)
                    {
                        tbEditable.Text = string.Empty;
                        tbEditable.CaretIndex = 0;
                    }
                }
                catch { }

                if (OrderList != null)
                {
                    foreach (var it in OrderList) ReleaseId(it.Id);
                    OrderList.Clear();
                }
                dtgdOrder.ItemsSource = OrderList;
                iudAmmount.Value = 1;
            }
            finally
            {
                _suspendCbOrderTextChanged = false;
            }

            // Remove focus so the editable textbox does not show caret/text again
            Keyboard.ClearFocus();
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
            if (id > 0 && id < MaxIds) _idUsed[id] = false;
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

        // Logic tìm kiếm không dấu
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private void cbOrder_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_suspendCbOrderTextChanged) return;

            var tb = sender as TextBox;
            if (tb == null || cbOrder == null) return;

            string currentText = tb.Text ?? string.Empty;
            int caretIndex = tb.CaretIndex;

            _suspendCbOrderTextChanged = true;
            try
            {
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

                var filtered = items.ToList();

                if (filtered.Count == 1 &&
                    string.Equals(RemoveDiacritics(filtered[0].Name).Trim(), RemoveDiacritics(currentText).Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var single = new List<Drink> { filtered[0] };
                    cbOrder.ItemsSource = single;
                    cbOrder.SelectedItem = single[0];

                    cbOrder.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            cbOrder.IsDropDownOpen = false;
                            var tbEditable = cbOrder.Template.FindName("PART_EditableTextBox", cbOrder) as TextBox;
                            if (tbEditable != null)
                            {
                                tbEditable.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                            }
                            else
                            {
                                Keyboard.ClearFocus();
                            }
                        }
                        catch { }
                    }), DispatcherPriority.Background);
                }
                else
                {
                    cbOrder.ItemsSource = filtered;
                    cbOrder.SelectedIndex = -1;
                    cbOrder.Dispatcher.BeginInvoke(new Action(() => cbOrder.IsDropDownOpen = filtered.Count > 0), DispatcherPriority.Background);
                }

                tb.Text = currentText;
                tb.CaretIndex = Math.Min(caretIndex, tb.Text.Length);
            }
            finally
            {
                _suspendCbOrderTextChanged = false;
            }
        }

        private void cbOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (cbOrder == null) return;
            var tb = cbOrder.Template.FindName("PART_EditableTextBox", cbOrder) as TextBox;
            if (tb != null)
            {
                tb.Clear();
                tb.Focus();
                cbOrder.IsDropDownOpen = false;
            }
        }

        private void InitializeDrinks()
        {
            try
            {
                var products = ProductDAO.Instance.GetListProduct();
                Drinks = products.Select(p => new Drink
                {
                    Id = p.Id,
                    Name = p.ProName ?? string.Empty,
                    Price = (int)Decimal.ToInt32(p.Price)
                }).ToList();
            }
            catch
            {
                // Fallback nếu mất kết nối DB
                Drinks = new List<Drink>
                {
                    new() { Id = 1, Name = "Cà phê đen", Price = 15000 },
                    new() { Id = 2, Name = "Cà phê sữa", Price = 17000 },
                    new() { Id = 3, Name = "Cà phê muối", Price = 20000 },
                    new() { Id = 4, Name = "Soda dâu", Price = 23000 }
                };
            }
        }

        private void InitializeTables()
        {
            if (File.Exists(_tablesStatePath))
            {
                try
                {
                    var json = File.ReadAllText(_tablesStatePath);
                    var states = JsonSerializer.Deserialize<List<TableState>>(json);
                    if (states != null && states.Count > 0)
                    {
                        Tables = new ObservableCollection<Table>(
                            states.Select(s =>
                            {
                                var t = new Table { Id = s.Id, Name = s.Name, Status = s.Status, EndTimeUtc = s.EndTimeUtc };
                                if (s.EndTimeUtc.HasValue)
                                    t.Countdown = (int)Math.Max(0, (s.EndTimeUtc.Value - DateTime.UtcNow).TotalSeconds);
                                else
                                    t.Countdown = 0;
                                return t;
                            }).OrderBy(t => t.Id)
                        );
                        return;
                    }
                }
                catch { }
            }

            // Default
            Tables = new ObservableCollection<Table>(
                Enumerable.Range(1, 12).Select(i => new Table { Id = i, Name = $"Bàn {i}", Status = "Free", Countdown = 0 })
            );
        }

        private void SaveTablesState()
        {
            try
            {
                var states = Tables.Select(t => new TableState(t.Id, t.Name ?? $"Bàn {t.Id}", t.Status, t.EndTimeUtc)).ToList();
                var json = JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_tablesStatePath, json);
            }
            catch { }
        }

        // Build VietQR quicklink URL with parameters
        private static string BuildVietQrUrl(string quickLinkBase, int amount, string addInfo, string accountName)
        {
            return $"{quickLinkBase}?amount={amount}&addInfo={Uri.EscapeDataString(addInfo)}&accountName={Uri.EscapeDataString(accountName)}";
        }

        // Show an inline dialog with the QR image. Returns true if user confirmed payment.
        private bool ShowVietQrDialog(string url, int amount, string addInfo, string accountName)
        {
            // Build UI dynamically
            var win = new Window()
            {
                Title = "VietQR - Quét để thanh toán",
                Width = 420,
                Height = 560,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this
            };

            var panel = new StackPanel() { Margin = new Thickness(10) };

            var txt = new TextBlock()
            {
                Text = $"Quét mã QR để thanh toán: {amount} VNĐ\n{addInfo}\nNgười nhận: {accountName}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            panel.Children.Add(txt);

            var img = new System.Windows.Controls.Image()
            {
                Width = 380,
                Height = 380,
                Stretch = System.Windows.Media.Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(url);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                img.Source = bmp;
            }
            catch
            {
                // If image fails to load, show a placeholder text
                panel.Children.Add(new TextBlock() { Text = "Không thể tải hình QR. Vui lòng kiểm tra mạng hoặc URL.", Foreground = System.Windows.Media.Brushes.Red });
            }

            panel.Children.Add(img);

            var btnPanel = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            var btnConfirm = new Button() { Content = "Đã thanh toán", Width = 120, Margin = new Thickness(6) };
            var btnCancel = new Button() { Content = "Hủy", Width = 120, Margin = new Thickness(6) };
            btnPanel.Children.Add(btnConfirm);
            btnPanel.Children.Add(btnCancel);
            panel.Children.Add(btnPanel);

            bool result = false;
            btnConfirm.Click += (s, e) =>
            {
                result = true;
                win.DialogResult = true;
                win.Close();
            };
            btnCancel.Click += (s, e) =>
            {
                result = false;
                win.DialogResult = false;
                win.Close();
            };

            win.Content = panel;
            win.ShowDialog();
            return result;
        }
    }
}