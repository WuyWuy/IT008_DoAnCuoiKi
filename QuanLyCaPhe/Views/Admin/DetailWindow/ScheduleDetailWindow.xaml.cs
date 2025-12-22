using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import Custom MessageBox

namespace QuanLyCaPhe.Views.Admin.DetailWindow
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

            for (int i = 6; i <= 23; i++) cbEndHour.Items.Add(i);
            cbEndMinute.Items.Add(0);
            cbEndMinute.Items.Add(30);

            int endH = _startTime.Hours + 4;
            if (endH > 22) endH = 22;

            cbEndHour.SelectedItem = endH;
            cbEndMinute.SelectedItem = _startTime.Minutes;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (cbName.SelectedItem == null)
            {
                JetMoonMessageBox.Show("Vui lòng chọn nhân viên!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            if (cbEndHour.SelectedItem == null || cbEndMinute.SelectedItem == null)
            {
                JetMoonMessageBox.Show("Vui lòng chọn giờ kết thúc!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            int userId = (int)cbName.SelectedValue;
            int h = (int)cbEndHour.SelectedItem;
            int m = (int)cbEndMinute.SelectedItem;
            TimeSpan endTime = new TimeSpan(h, m, 0);

            if (endTime <= _startTime)
            {
                JetMoonMessageBox.Show("Giờ kết thúc phải lớn hơn giờ bắt đầu!", "Lỗi thời gian", MsgType.Warning);
                return;
            }

            try
            {
                if (WorkScheduleDAO.Instance.RegisterSchedule(userId, _date, _startTime, endTime, ""))
                {
                    JetMoonMessageBox.Show("Đăng ký ca làm thành công!", "Hoàn tất", MsgType.Success);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    JetMoonMessageBox.Show("Không thể lưu vào CSDL (Có thể do trùng ca).", "Lỗi", MsgType.Error);
                }
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MsgType.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}