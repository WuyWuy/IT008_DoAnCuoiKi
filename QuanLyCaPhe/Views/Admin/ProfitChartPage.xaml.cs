using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class ProfitChartPage : Page
    {
        public SeriesCollection ThreeLineSeries { get; set; }
        public string[] Months { get; set; }
        public Func<double, string> CurrencyFormatter { get; set; }

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
            Months = new[] { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6" };

            ThreeLineSeries = new SeriesCollection
            {
                // ĐƯỜNG THU NHẬP (Xanh lá - #10B981)
                new LineSeries
                {
                    Title = "Thu nhập",
                    Values = new ChartValues<decimal>
                    {
                        12.4m, 13.5m, 14.2m, 12.8m, 15.1m, 16.3m
                    },
                    Stroke = new SolidColorBrush(Color.FromRgb(16, 185, 129)), // #10B981
                    Fill = Brushes.Transparent,
                    StrokeThickness = 3,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    PointForeground = Brushes.White
                },
                
                // ĐƯỜNG CHI PHÍ (Đỏ - #EF4444)
                new LineSeries
                {
                    Title = "Chi phí",
                    Values = new ChartValues<decimal>
                    {
                        8.2m, 7.8m, 8.1m, 8.5m, 9.2m, 9.8m
                    },
                    Stroke = new SolidColorBrush(Color.FromRgb(239, 68, 68)), // #EF4444
                    Fill = Brushes.Transparent,
                    StrokeThickness = 3,
                    StrokeDashArray = new DoubleCollection { 4 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 8
                },
                
                // ĐƯỜNG LỢI NHUẬN (Xanh dương - #3B82F6)
                new LineSeries
                {
                    Title = "Lợi nhuận",
                    Values = new ChartValues<decimal>
                    {
                        4.2m, 5.7m, 6.1m, 4.3m, 5.9m, 6.5m
                    },
                    Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)), // #3B82F6
                    Fill = Brushes.Transparent,
                    StrokeThickness = 3,
                    PointGeometry = DefaultGeometries.Triangle,
                    PointGeometrySize = 10
                }
            };

            CurrencyFormatter = value => value.ToString("N1") + "M ₫";
        }
    }
}