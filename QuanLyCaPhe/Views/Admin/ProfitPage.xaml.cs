using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System.ComponentModel;
using System.Globalization;

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for ProfitPage.xaml
    /// </summary>
    public partial class ProfitPage : Page
    {
        public ProfitPage()
        {
            InitializeComponent();
            ChartFrame.Navigate(new ProfitChartPage());

            // Listen to changes on DatePickerV2.SelectedDate dependency property
            var dpd = DependencyPropertyDescriptor.FromProperty(
                Views.Components.DatePickerv2.SelectedDateProperty,
                typeof(Views.Components.DatePickerv2));

            if (dpd != null)
            {
                dpd.AddValueChanged(DatePickerV2, DatePicker_SelectedDateChanged);
            }

            // Initial update
            UpdateTotals();
        }

        private void DatePicker_SelectedDateChanged(object? sender, EventArgs e)
        {
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            try
            {
                DateTime selected = DatePickerV2.SelectedDate;
                int month = selected.Month;
                int year = selected.Year;

                // Total revenue: sum of TotalPrice from bills in selected month/year
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

                // Total expense: sum of InputPrice from input infos in selected month/year
                decimal totalExpense = 0m;
                var inputs = InputInfoDAO.Instance.GetListInputInfo();
                foreach (var inp in inputs)
                {
                    if (inp.DateInput.Month == month && inp.DateInput.Year == year)
                    {
                        totalExpense += inp.InputPrice;
                    }
                }

                // Labor cost: sum of worked hours * user's hourly wage for schedules in the month
                decimal totalLabor = 0m;

                // Build a lookup of user hourly wages
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
                        // Calculate hours worked
                        double hoursDouble = (s.EndTime - s.StartTime).TotalHours;
                        if (hoursDouble <= 0) continue; // skip invalid or zero-length shifts

                        decimal hours = (decimal)hoursDouble;

                        if (wageById.TryGetValue(s.UserId, out decimal hourlyWage))
                        {
                            totalLabor += hours * hourlyWage;
                        }
                    }
                }

                // Add labor cost to total expense
                totalExpense += totalLabor;

                decimal profit = totalRevenue - totalExpense;

                TotalRevenueText.Text = FormatCurrencyVnd(totalRevenue);
                TotalExpenseText.Text = FormatCurrencyVnd(totalExpense);
                ProfitText.Text = FormatCurrencyVnd(profit);
            }
            catch
            {
                // ignore errors silently to avoid breaking UI
            }
        }

        private string FormatCurrencyVnd(decimal amount)
        {
            // Format with thousand separators and no decimals, append currency symbol
            return string.Format(CultureInfo.InvariantCulture, "{0:N0}đ", amount);
        }
    }
}
