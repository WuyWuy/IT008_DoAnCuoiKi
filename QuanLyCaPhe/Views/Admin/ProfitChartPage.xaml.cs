using System;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Globalization;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class ProfitChartPage : Page, INotifyPropertyChanged
    {
        public SeriesCollection ThreeLineSeries { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> CurrencyFormatter { get; set; }
        public string UnitText { get; set; }

        public ProfitChartPage()
        {
            InitializeComponent();
            ThreeLineSeries = new SeriesCollection();

            // Mặc định formatter
            CurrencyFormatter = value => value.ToString("N0");
            DataContext = this;
        }

        public void UpdateChart(DateTime selectedDate)
        {
            try
            {
                int month = selectedDate.Month;
                int year = selectedDate.Year;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                var dayLabels = new List<string>();
                for (int d = 1; d <= daysInMonth; d++) dayLabels.Add(d.ToString());
                Labels = dayLabels.ToArray();

                // Lấy dữ liệu
                var bills = BillDAO.Instance.GetListBills();
                var inputs = InputInfoDAO.Instance.GetListInputInfo();
                var users = UserDAO.Instance.GetListUser();

                var wageById = new Dictionary<int, decimal>();
                foreach (var u in users) { if (!wageById.ContainsKey(u.Id)) wageById.Add(u.Id, u.HourlyWage); }

                var revenueValues = new ChartValues<decimal>();
                var expenseValues = new ChartValues<decimal>();
                var profitValues = new ChartValues<decimal>();

                decimal globalMax = 0m;

                for (int d = 1; d <= daysInMonth; d++)
                {
                    DateTime currentDate = new DateTime(year, month, d);
                    decimal dailyRevenue = 0m;
                    decimal dailyInputExpense = 0m;
                    decimal dailyLabor = 0m;

                    // Tính doanh thu
                    foreach (var b in bills)
                    {
                        DateTime checkDate = b.DateCheckOut ?? b.DateCheckIn;
                        if (checkDate.Date == currentDate.Date) dailyRevenue += b.TotalPrice;
                    }

                    // Tính chi nhập hàng
                    foreach (var inp in inputs)
                    {
                        if (inp.DateInput.Date == currentDate.Date) dailyInputExpense += inp.InputPrice;
                    }

                    // Tính chi lương
                    var schedules = WorkScheduleDAO.Instance.GetListByDate(currentDate);
                    foreach (var s in schedules)
                    {
                        double hours = (s.EndTime - s.StartTime).TotalHours;
                        if (hours > 0 && wageById.TryGetValue(s.UserId, out decimal wage))
                            dailyLabor += (decimal)hours * wage;
                    }

                    decimal dailyTotalExpense = dailyInputExpense + dailyLabor;
                    decimal dailyProfit = dailyRevenue - dailyTotalExpense;

                    // Tìm max để format trục Y
                    globalMax = Math.Max(globalMax, Math.Max(dailyRevenue, Math.Max(dailyTotalExpense, Math.Abs(dailyProfit))));

                    // [QUAN TRỌNG] Add giá trị THỰC, không chia scale ở đây để Tooltip hiện đúng số tiền
                    revenueValues.Add(dailyRevenue);
                    expenseValues.Add(dailyTotalExpense);
                    profitValues.Add(dailyProfit);
                }

                // Xử lý đơn vị hiển thị TRÊN TRỤC Y (Axis)
                decimal scaleDivisor = 1m;
                if (globalMax >= 1_000_000m)
                {
                    scaleDivisor = 1_000_000m;
                    UnitText = $"(Triệu đồng - {month}/{year})";
                    CurrencyFormatter = val => (val / (double)scaleDivisor).ToString("N1") + "M";
                }
                else if (globalMax >= 1_000m)
                {
                    scaleDivisor = 1_000m;
                    UnitText = $"(Nghìn đồng - {month}/{year})";
                    CurrencyFormatter = val => (val / (double)scaleDivisor).ToString("N0") + "K";
                }
                else
                {
                    UnitText = $"(Đồng - {month}/{year})";
                    CurrencyFormatter = val => val.ToString("N0");
                }

                // [FIX] Cấu hình Series
                // 1. LabelPoint: Định dạng chữ hiển thị trong Tooltip (vd: 1,500,000đ)
                Func<ChartPoint, string> labelPoint = chartPoint =>
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}đ", chartPoint.Y);

                ThreeLineSeries = new SeriesCollection
                {
                    // Series 1: THU (Ẩn hình ảnh, nhưng vẫn có dữ liệu để hiện Tooltip)
                    new LineSeries
                    {
                        Title = "Thu",
                        Values = revenueValues,
                        LabelPoint = labelPoint,
                        StrokeThickness = 0,             // Ẩn đường
                        Fill = Brushes.Transparent,      // Ẩn vùng
                        PointGeometry = null,            // Ẩn điểm
                        LineSmoothness = 0
                    },

                    // Series 2: CHI (Ẩn hình ảnh)
                    new LineSeries
                    {
                        Title = "Chi",
                        Values = expenseValues,
                        LabelPoint = labelPoint,
                        StrokeThickness = 0,
                        Fill = Brushes.Transparent,
                        PointGeometry = null,
                        LineSmoothness = 0
                    },

                    // Series 3: LỜI (Hiển thị)
                    new LineSeries
                    {
                        Title = "Lời",
                        Values = profitValues,
                        LabelPoint = labelPoint,
                        Stroke = new SolidColorBrush(Color.FromRgb(59,130,246)), // Màu xanh dương đẹp
                        Fill = new SolidColorBrush(Color.FromArgb(30, 59, 130, 246)), // Fill nhẹ bên dưới
                        PointGeometrySize = 10, // Điểm to rõ
                        LineSmoothness = 0.5    // Đường cong mềm
                    }
                };

                OnPropertyChanged(nameof(ThreeLineSeries));
                OnPropertyChanged(nameof(Labels));
                OnPropertyChanged(nameof(UnitText));
                OnPropertyChanged(nameof(CurrencyFormatter));
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}