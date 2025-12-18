using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // Dùng MessageBox Custom

namespace QuanLyCaPhe.Views.SharedPage
{
    public partial class ScheduleRegisterWindow : Window
    {
        private DateTime _date;
        private TimeSpan _startTime;

        public ScheduleRegisterWindow(DateTime date, TimeSpan startTime)
        {
            InitializeComponent();
            _date = date;
            _startTime = startTime;

            // Hiển thị thông tin
            txtDate.Text = _date.ToString("dd/MM/yyyy");
            txtStartTime.Text = _startTime.ToString(@"hh\:mm");

            // Load dữ liệu
            LoadStaff();
            InitTimeCombos();
        }

        private void LoadStaff()
        {
            try
            {
                var users = UserDAO.Instance.GetListUser();
                // Chỉ lấy nhân viên Staff đang hoạt động
                var staffUsers = users.Where(u => u.RoleName == "Staff" && u.IsActive).ToList();

                cbName.ItemsSource = staffUsers;
                cbName.DisplayMemberPath = "FullName";
                cbName.SelectedValuePath = "Id";
            }
            catch { }
        }

        private void InitTimeCombos()
        {
            cbEndHour.Items.Clear();
            cbEndMinute.Items.Clear();

            // Giờ từ 6h đến 23h
            for (int i = 6; i <= 23; i++) cbEndHour.Items.Add(i);
            cbEndMinute.Items.Add(0);
            cbEndMinute.Items.Add(30);

            // Mặc định chọn giờ kết thúc = Bắt đầu + 4 tiếng
            int endH = _startTime.Hours + 4;
            if (endH > 22) endH = 22;

            cbEndHour.SelectedItem = endH;
            cbEndMinute.SelectedItem = _startTime.Minutes;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra nhập liệu
            if (cbName.SelectedItem == null)
            {
                txtError.Text = "Vui lòng chọn nhân viên!";
                return;
            }

            if (cbEndHour.SelectedItem == null || cbEndMinute.SelectedItem == null)
            {
                txtError.Text = "Vui lòng chọn giờ kết thúc!";
                return;
            }

            int userId = (int)cbName.SelectedValue;
            int h = (int)cbEndHour.SelectedItem;
            int m = (int)cbEndMinute.SelectedItem;
            TimeSpan endTime = new TimeSpan(h, m, 0);

            if (endTime <= _startTime)
            {
                txtError.Text = "Giờ kết thúc phải lớn hơn giờ bắt đầu!";
                return;
            }

            // 2. Lưu vào CSDL
            try
            {
                // Gọi DAO để lưu (Hàm RegisterSchedule cần kiểm tra trùng ca bên trong hoặc ở đây)
                if (WorkScheduleDAO.Instance.RegisterSchedule(userId, _date, _startTime, endTime, ""))
                {
                    JetMoonMessageBox.Show("Đăng ký ca làm thành công!", "Hoàn tất", MsgType.Success);
                    this.DialogResult = true; // Báo cho cửa sổ cha là OK
                    this.Close();
                }
                else
                {
                    txtError.Text = "Lỗi: Không thể lưu vào CSDL (Có thể do trùng ca).";
                }
            }
            catch (Exception ex)
            {
                txtError.Text = "Lỗi hệ thống: " + ex.Message;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}