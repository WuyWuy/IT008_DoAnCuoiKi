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
using System.Windows.Threading;
using QuanLyCaPhe.DAO;

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {

        private DispatcherTimer _timer;

        public HomePage()
        {
            InitializeComponent();

            // Cấu hình Timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5); // 5 giây load 1 lần
            _timer.Tick += (s, e) => LoadOverviewData(); // Cứ hết giờ là gọi hàm Load

            // Khi trang hiện lên thì BẮT ĐẦU đếm giờ
            this.Loaded += (s, e) => {
                LoadOverviewData(); // Load ngay lập tức 1 phát đầu tiên
                _timer.Start();
            };

            // Khi rời khỏi trang thì DỪNG đếm giờ (để tiết kiệm RAM/CPU)
            this.Unloaded += (s, e) => _timer.Stop();

            this.Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOverviewData();
            MiniChart.Navigate(new ProfitChartPage());
            LastBill.Navigate(new LastBillPage());

        }

        private void LoadOverviewData()
        {
            try
            {
                // --- PHẦN 1: LẤY DỮ LIỆU (Giả lập - Bạn thay thế bằng code gọi Database thật ở đây) ---

                // Ví dụ: int productCount = DataProvider.Ins.DB.Products.Count();
                int productCount = ProductDAO.Instance.GetCountProduct();

                // Ví dụ: int staffCount = DataProvider.Ins.DB.Staffs.Count();
                // Use GetCountStaff to get number of staff users
                int staffCount = UserDAO.Instance.GetCountStaff();

                // compute totals for current month (same logic as ProfitPage)
                DateTime now = DateTime.Now;
                int month = now.Month;
                int year = now.Year;

                decimal totalIncome =0m;
                var bills = BillDAO.Instance.GetListBills();
                foreach (var b in bills)
                {
                    DateTime checkDate = b.DateCheckOut ?? b.DateCheckIn;
                    if (checkDate.Month == month && checkDate.Year == year)
                    {
                        totalIncome += b.TotalPrice;
                    }
                }

                decimal totalInputExpense =0m;
                var inputs = InputInfoDAO.Instance.GetListInputInfo();
                foreach (var inp in inputs)
                {
                    if (inp.DateInput.Month == month && inp.DateInput.Year == year)
                    {
                        totalInputExpense += inp.InputPrice;
                    }
                }

                // labor cost from schedules
                decimal totalLabor =0m;
                var users = UserDAO.Instance.GetListUser();
                var wageById = users.ToDictionary(u => u.Id, u => u.HourlyWage);

                DateTime firstDay = new DateTime(year, month,1);
                int daysInMonth = DateTime.DaysInMonth(year, month);
                for (int d =0; d < daysInMonth; d++)
                {
                    DateTime date = firstDay.AddDays(d);
                    var schedules = WorkScheduleDAO.Instance.GetListByDate(date);
                    foreach (var s in schedules)
                    {
                        double hoursDouble = (s.EndTime - s.StartTime).TotalHours;
                        if (hoursDouble <=0) continue;
                        decimal hours = (decimal)hoursDouble;
                        if (wageById.TryGetValue(s.UserId, out decimal hourlyWage))
                        {
                            totalLabor += hours * hourlyWage;
                        }
                    }
                }

                decimal totalExpense = totalInputExpense + totalLabor;


                // --- PHẦN 2: HIỂN THỊ LÊN GIAO DIỆN ---

                // 1. Tổng món ăn
                if (tbProductCount != null)
                    tbProductCount.Text = productCount.ToString();

                // 2. Nhân viên
                if (tbStaffCount != null)
                    tbStaffCount.Text = staffCount.ToString();

                // 3. Thu nhập (Format tiền tệ: 15,600,000 đ)
                if (tbIncome != null)
                    tbIncome.Text = string.Format("{0:N0} đ", totalIncome);

                // 4. Chi tiêu
                if (tbExpense != null)
                    tbExpense.Text = string.Format("{0:N0} đ", totalExpense);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu tổng quan: " + ex.Message);
            }
        }
    }
}
