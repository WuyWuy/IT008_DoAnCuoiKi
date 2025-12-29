using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // <-- Import Custom MessageBox
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using QuanLyCaPhe.Services;
using System.Linq;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class UserDetailWindow : Window
    {
        private User _user;
        private TextBox? _wageBox;
        private ComboBoxItem? _staffItem;

        public UserDetailWindow()
        {
            InitializeComponent();
            _user = null;
            txtEmail.IsEnabled = true;
            pnlPassword.Visibility = Visibility.Visible;

            // Cache wage box and staff item, attach handler to keep UI consistent
            _wageBox = this.FindName("txtHourlyWage") as TextBox;
            _staffItem = cboRole.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (i.Tag?.ToString() ?? "") == "Staff");
            cboRole.SelectionChanged += CboRole_SelectionChanged;
            // default: if role is Admin selected, make wage read-only (rare for create)
            if ((cboRole.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Admin")
                if (_wageBox != null) _wageBox.IsReadOnly = true;
        }

        public UserDetailWindow(User user)
        {
            InitializeComponent();
            _user = user;

            txtFullName.Text = user.FullName;
            txtEmail.Text = user.Email;
            txtPhone.Text = user.Phone;
            txtAddress.Text = user.Address;

            _wageBox = this.FindName("txtHourlyWage") as TextBox;
            if (_wageBox != null)
            {
                _wageBox.Text = user.HourlyWage.ToString("N0");
            }

            foreach (ComboBoxItem item in cboRole.Items)
            {
                if (item.Content.ToString() == user.RoleName)
                {
                    cboRole.SelectedItem = item;
                    break;
                }
            }

            txtEmail.IsEnabled = false;
            pnlPassword.Visibility = Visibility.Collapsed;

            // Cache staff item and attach handler
            _staffItem = cboRole.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (i.Tag?.ToString() ?? "") == "Staff");
            cboRole.SelectionChanged += CboRole_SelectionChanged;

            // 1) If editing an Admin, make hourly wage read-only by default
            if (user.RoleName == "Admin" && _wageBox != null)
            {
                _wageBox.IsReadOnly = true;
            }

            // 2) If this is the last remaining Admin, disallow selecting "Staff" (prevent demotion)
            try
            {
                if (user.RoleName == "Admin" && UserDAO.Instance.GetCountAdmin() <= 1)
                {
                    if (_staffItem != null)
                    {
                        _staffItem.IsEnabled = false;
                        _staffItem.ToolTip = "Không thể hạ vai trò vì đây là Admin duy nhất.";
                    }
                    // Optional: also prevent cboRole being changed entirely
                    // cboRole.IsEnabled = false;
                }
            }
            catch
            {
                // ignore counting errors — fail-safe: keep controls enabled
            }
        }

        private void CboRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Keep hourly wage readonly when selected role is Admin; editable otherwise.
            var selectedRole = (cboRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Staff";
            if (_wageBox != null)
            {
                _wageBox.IsReadOnly = selectedRole == "Admin";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                JetMoonMessageBox.Show("Vui lòng nhập tên và email!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            string name = txtFullName.Text;
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text;
            string address = txtAddress.Text;
            string gender = cboGender.Text;
            string role = (cboRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Staff";

            decimal hourly = 0m;
            var wageBox = this.FindName("txtHourlyWage") as TextBox;
            string wageText = wageBox?.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(wageText))
            {
                if (!decimal.TryParse(wageText, out hourly))
                {
                    JetMoonMessageBox.Show("Lương theo giờ không hợp lệ. Vui lòng nhập số.", "Lỗi nhập liệu", MsgType.Error);
                    return;
                }
            }

            const string emailPattern = "^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                JetMoonMessageBox.Show("Email không đúng định dạng (xxx@xxx.com).", "Lỗi định dạng", MsgType.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                if (_user == null)
                {
                    if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        JetMoonMessageBox.Show("Vui lòng nhập mật khẩu khởi tạo!", "Thiếu thông tin", MsgType.Warning);
                        return;
                    }

                    if (UserDAO.Instance.GetIdByEmail(email) != -1)
                    {
                        JetMoonMessageBox.Show("Email này đã tồn tại trong hệ thống!", "Trùng lặp", MsgType.Error);
                        return;
                    }

                    string password = txtPassword.Password;

                    if (UserDAO.Instance.InsertUser(name, email, phone, address, gender, password, role, hourly))
                    {
                        try
                        {
                            GlobalService.RecordActivity("Nhân viên", "Thêm nhân viên", $"Đã thêm nhân viên: {name} ({email})");
                        }
                        catch { }

                        JetMoonMessageBox.Show("Thêm nhân viên thành công!", "Hoàn tất", MsgType.Success);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Thêm nhân viên thất bại!", "Lỗi", MsgType.Error);
                    }
                }
                else
                {
                    // capture old values for activity details
                    var oldName = _user.FullName;
                    var oldPhone = _user.Phone;
                    var oldAddress = _user.Address;
                    var oldGender = _user.Gender;
                    var oldRole = _user.RoleName;
                    var oldHourly = _user.HourlyWage;

                    // Prevent demoting the last admin
                    if (oldRole == "Admin" && role == "Staff")
                    {
                        try
                        {
                            if (UserDAO.Instance.GetCountAdmin() <= 1)
                            {
                                JetMoonMessageBox.Show("Không thể chuyển vai trò vì chỉ còn 1 Admin trong hệ thống.", "Hành động bị chặn", MsgType.Warning);
                                return;
                            }
                        }
                        catch
                        {
                            // if counting fails, block the demotion as a safe default
                            JetMoonMessageBox.Show("Không thể kiểm tra số lượng Admin. Hành động bị chặn.", "Lỗi", MsgType.Warning);
                            return;
                        }
                    }

                    if (UserDAO.Instance.UpdateUser(_user.Id, name, phone, address, gender, role, hourly))
                    {
                        try
                        {
                            var changes = new System.Collections.Generic.List<string>();
                            if (oldName != name) changes.Add($"Tên: \"{oldName}\" → \"{name}\"");
                            if (oldPhone != phone) changes.Add($"Điện thoại: \"{oldPhone}\" → \"{phone}\"");
                            if (oldAddress != address) changes.Add("Địa chỉ: đã thay đổi");
                            if (oldGender != gender) changes.Add($"Giới tính: \"{oldGender}\" → \"{gender}\"");
                            if (oldRole != role) changes.Add($"Vai trò: \"{oldRole}\" → \"{role}\"");
                            if (oldHourly != hourly) changes.Add($"Lương theo giờ: \"{oldHourly}\" → \"{hourly}\"");

                            var detail = changes.Count > 0 ? string.Join("; ", changes) : "Không có thay đổi nội dung";
                            GlobalService.RecordActivity("Nhân viên", "Cập nhật nhân viên", $"Nhân viên #{_user.Id}: {detail}");
                        }
                        catch { }

                        JetMoonMessageBox.Show("Cập nhật hồ sơ thành công!", "Hoàn tất", MsgType.Success);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Cập nhật thất bại!", "Lỗi", MsgType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MsgType.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}