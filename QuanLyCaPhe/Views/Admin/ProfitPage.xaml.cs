using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System.ComponentModel;
using System.Globalization;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class ProfitPage : Page
    {
        // [FIX] Tạo biến lưu giữ trang biểu đồ để gọi update bất cứ lúc nào
        private readonly ProfitChartPage _chartPage = new ProfitChartPage();

        public ProfitPage()
        {
            InitializeComponent();

            // 1. Điều hướng Frame tới biến _chartPage đã tạo
            ChartFrame.Navigate(_chartPage);

            // 2. Lắng nghe sự kiện DatePicker
            var dpd = DependencyPropertyDescriptor.FromProperty(
                Views.Components.DatePickerv2.SelectedDateProperty,
                typeof(Views.Components.DatePickerv2));

            if (dpd != null)
            {
                dpd.AddValueChanged(DatePickerV2, DatePicker_SelectedDateChanged);
            }

            // 3. Load dữ liệu khi trang ProfitPage hiển thị
            this.Loaded += (s, e) => UpdateAllData();
        }

        private void DatePicker_SelectedDateChanged(object? sender, EventArgs e)
        {
            UpdateAllData();
        }

        private void UpdateAllData()
        {
            // A. Cập nhật thẻ tiền
            UpdateTotals();

            // B. [FIX] Gọi trực tiếp vào biến _chartPage, không cần quan tâm Frame load xong chưa
            if (DatePickerV2.SelectedDate != DateTime.MinValue)
            {
                _chartPage.UpdateChart(DatePickerV2.SelectedDate);
            }
        }

        private void UpdateTotals()
        {
            try
            {
                DateTime selected = DatePickerV2.SelectedDate;
                int month = selected.Month;
                int year = selected.Year;

                // --- 1. Tổng Thu ---
                decimal totalRevenue = 0m;
                var bills = BillDAO.Instance.GetListBills();
                foreach (var b in bills)
                {
                    DateTime checkDate = b.DateCheckOut ?? b.DateCheckIn;
                    if (checkDate.Month == month && checkDate.Year == year)
                    {
                        totalRevenue += b.TotalPrice;
                    }
                }

                // --- 2. Tổng Chi (Nhập hàng) ---
                decimal totalExpense = 0m;
                var inputs = InputInfoDAO.Instance.GetListInputInfo();
                foreach (var inp in inputs)
                {
                    if (inp.DateInput.Month == month && inp.DateInput.Year == year)
                    {
                        totalExpense += inp.InputPrice;
                    }
                }

                // --- 3. Tổng Chi (Lương) ---
                decimal totalLabor = 0m;
                var users = UserDAO.Instance.GetListUser();
                var wageById = users.ToDictionary(u => u.Id, u => u.HourlyWage);

                DateTime firstDay = new DateTime(year, month, 1);
                int daysInMonth = DateTime.DaysInMonth(year, month);

                for (int d = 0; d < daysInMonth; d++)
                {
                    DateTime date = firstDay.AddDays(d);
                    var schedules = WorkScheduleDAO.Instance.GetListByDate(date);
                    foreach (var s in schedules)
                    {
                        double hoursDouble = (s.EndTime - s.StartTime).TotalHours;
                        if (hoursDouble <= 0) continue;
                        if (wageById.TryGetValue(s.UserId, out decimal hourlyWage))
                        {
                            totalLabor += (decimal)hoursDouble * hourlyWage;
                        }
                    }
                }

                totalExpense += totalLabor;
                decimal profit = totalRevenue - totalExpense;

                TotalRevenueText.Text = FormatCurrencyVnd(totalRevenue);
                TotalExpenseText.Text = FormatCurrencyVnd(totalExpense);
                ProfitText.Text = FormatCurrencyVnd(profit);
            }
            catch { }
        }

        private string FormatCurrencyVnd(decimal amount)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:N0}đ", amount);
        }
    }
}