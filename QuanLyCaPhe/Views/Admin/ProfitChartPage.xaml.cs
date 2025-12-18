using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class ProfitChartPage : Page
    {
        public SeriesCollection ThreeLineSeries { get; set; }
        public string[] Months { get; set; }
        public Func<double, string> CurrencyFormatter { get; set; }
        public string UnitText { get; set; }

        public ProfitChartPage()
        {
            InitializeComponent();
            Loaded += ProfitChartPage_Loaded;
        }

        private void ProfitChartPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeChartData();
            DataContext = this;
        }

        private void InitializeChartData()
        {
            // Show most recent 6 months until now
            DateTime now = DateTime.Now;
            int monthsToShow = 6;
            var monthList = new List<int>();
            var yearList = new List<int>();
            for (int i = monthsToShow - 1; i >= 0; i--)
            {
                var dt = now.AddMonths(-i);
                monthList.Add(dt.Month);
                yearList.Add(dt.Year);
            }

            Months = monthList.Select((m, idx) => $"Tháng {monthList[idx]}").ToArray();

            // Prepare lookup for users' hourly wage
            var users = UserDAO.Instance.GetListUser();
            var wageById = users.ToDictionary(u => u.Id, u => u.HourlyWage);

            // Fetch all bills and inputs once
            var bills = BillDAO.Instance.GetListBills();
            var inputs = InputInfoDAO.Instance.GetListInputInfo();

            var revenueValues = new ChartValues<decimal>();
            var expenseValues = new ChartValues<decimal>();
            var profitValues = new ChartValues<decimal>();

            decimal globalMax = 0m;

            for (int i = 0; i < monthsToShow; i++)
            {
                int month = monthList[i];
                int year = yearList[i];

                decimal totalRevenue = 0m;
                decimal totalInputExpense = 0m;
                decimal totalLabor = 0m;

                // Sum bills for month
                foreach (var b in bills)
                {
                    DateTime checkDate = b.DateCheckOut ?? b.DateCheckIn;
                    if (checkDate.Month == month && checkDate.Year == year)
                    {
                        totalRevenue += b.TotalPrice;
                    }
                }

                // Sum inputs for month
                foreach (var inp in inputs)
                {
                    if (inp.DateInput.Month == month && inp.DateInput.Year == year)
                    {
                        totalInputExpense += inp.InputPrice;
                    }
                }

                // Labor: iterate days in month and get schedules
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
                        decimal hours = (decimal)hoursDouble;
                        if (wageById.TryGetValue(s.UserId, out decimal hourlyWage))
                        {
                            totalLabor += hours * hourlyWage;
                        }
                    }
                }

                decimal totalExpense = totalInputExpense + totalLabor;
                decimal profit = totalRevenue - totalExpense;

                // track max among raw values
                globalMax = Math.Max(globalMax, Math.Max(totalRevenue, Math.Max(totalExpense, Math.Abs(profit))));

                revenueValues.Add(totalRevenue);
                expenseValues.Add(totalExpense);
                profitValues.Add(profit);
            }

            // Determine unit based on globalMax
            // hundreds (100) -> display as 'Trăm' but likely not practical; we'll map to: hundreds(<=1_000), thousands(<=1_000_000), millions otherwise
            decimal scale = 1m;
            if (globalMax >= 1_000_000m)
            {
                // show in millions
                scale = 1_000_000m;
                UnitText = "(Đơn vị: Triệu đồng)";
                CurrencyFormatter = value => value.ToString("N1") + "M ₫";
            }
            else if (globalMax >= 1_000m)
            {
                // show in thousands
                scale = 1_000m;
                UnitText = "(Đơn vị: Nghìn đồng)";
                CurrencyFormatter = value => value.ToString("N0") + "K ₫";
            }
            else
            {
                // show in hundreds or units
                scale = 1m;
                UnitText = "(Đơn vị: Đồng)";
                CurrencyFormatter = value => value.ToString("N0") + " đ";
            }

            // Scale values for chart (chart shows numbers divided by scale)
            var scaledRevenue = new ChartValues<decimal>(revenueValues.Select(v => decimal.Round(v / scale, 2)));
            var scaledExpense = new ChartValues<decimal>(expenseValues.Select(v => decimal.Round(v / scale, 2)));
            var scaledProfit = new ChartValues<decimal>(profitValues.Select(v => decimal.Round(v / scale, 2)));

            ThreeLineSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Thu nhập",
                    Values = scaledRevenue,
                    Stroke = new SolidColorBrush(Color.FromRgb(16,185,129)), // #10B981
                    Fill = Brushes.Transparent,
                    StrokeThickness =3,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize =10,
                    PointForeground = Brushes.White
                },
                new LineSeries
                {
                    Title = "Chi phí",
                    Values = scaledExpense,
                    Stroke = new SolidColorBrush(Color.FromRgb(239,68,68)), // #EF4444
                    Fill = Brushes.Transparent,
                    StrokeThickness =3,
                    StrokeDashArray = new DoubleCollection {4 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize =8
                },
                new LineSeries
                {
                    Title = "Lợi nhuận",
                    Values = scaledProfit,
                    Stroke = new SolidColorBrush(Color.FromRgb(59,130,246)), // #3B82F6
                    Fill = Brushes.Transparent,
                    StrokeThickness =3,
                    PointGeometry = DefaultGeometries.Triangle,
                    PointGeometrySize =10
                }
            };

            // If CurrencyFormatter not set above for some reason, default to raw
            if (CurrencyFormatter == null)
            {
                CurrencyFormatter = value => value.ToString("N0");
            }
        }
    }
}