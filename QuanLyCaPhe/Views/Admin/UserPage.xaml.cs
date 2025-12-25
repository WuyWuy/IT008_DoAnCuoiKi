using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using QuanLyCaPhe.Helpers;
using Microsoft.Win32;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using QuanLyCaPhe.Views.Components; // <-- Import Custom MessageBox
using QuanLyCaPhe.Services;
using QuanLyCaPhe.DataAccess;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class UserPage : Page
    {
        // --- Khởi tạo & load ---
        public List<User> UserList { get; set; }

        public UserPage()
        {
            InitializeComponent();
            Loaded += StaffsPage_Loaded;
        }

        private void StaffsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            UserList = UserDAO.Instance.GetListUser();
            Usersdg.ItemsSource = UserList;
        }

        // --- Đếm STT --- 
        private void Usersdg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        // --- Tìm kiếm ---
        private void SearchBar_Clicked(object sender, string e)
        {
            Usersdg.ItemsSource = UserDAO.Instance.SearchUserByAll(SearchBar.Text);
        }

        // --- Thêm Mới ---
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var f = new UserDetailWindow();
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Sửa ---
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedUser = menuItem?.DataContext as User;

            if (selectedUser == null)
            {
                JetMoonMessageBox.Show("Vui lòng chọn dòng cần sửa!", "Chưa chọn dòng", MsgType.Warning);
                return;
            }

            var f = new UserDetailWindow(selectedUser);
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Xóa ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedUser = menuItem?.DataContext as User;

            if (selectedUser == null) return;

            // Prevent deleting the last Admin
            try
            {
                if (selectedUser.RoleName == "Admin")
                {
                    int adminCount = UserDAO.Instance.GetCountAdmin();
                    if (adminCount <= 1)
                    {
                        JetMoonMessageBox.Show("Không thể xóa. Phải giữ ít nhất 1 Admin.", "Cảnh báo", MsgType.Warning);
                        return;
                    }
                }
            }
            catch
            {
                // If DB check fails, continue
            }

            var result = JetMoonMessageBox.Show(
                $"Bạn có chắc muốn xóa nhân viên '{selectedUser.FullName}' không?",
                "Xác nhận xóa",
                MsgType.Question,
                true);

            if (result == true)
            {
                if (UserDAO.Instance.DeleteUser(selectedUser.Id))
                {
                    // Record activity
                    try
                    {
                        GlobalService.RecordActivity("Delete", "Xóa nhân viên", $"Đã xóa nhân viên {selectedUser.FullName}");
                    }
                    catch { }

                    JetMoonMessageBox.Show("Đã xóa thành công!", "Hoàn tất", MsgType.Success);
                    LoadData();
                }
                else
                {
                    JetMoonMessageBox.Show("Không thể xóa nhân viên này.", "Lỗi", MsgType.Error);
                }
            }
        }

        // --- Click nút Import/Export Excel ---
        private void IETable_Clicked(object sender, string e)
        {
            if (e == "Import") ImportData();
            else ExportData();
        }
        // --- Import ---
        private void ImportData()
        {
            var ofd = new OpenFileDialog() { Filter = "Excel Files|*.xlsx" };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    List<User> importedList = ExcelHelper.ImportList<User>(ofd.FileName);

                    // Validate format / contract before processing
                    if (!ValidateImportedUsers(importedList, out var validationError))
                    {
                        JetMoonMessageBox.Show(validationError, "Lỗi định dạng Excel", MsgType.Error);
                        return;
                    }

                    // Defensive cleanup: remove fully empty rows and rows missing required fields
                    var cleaned = new List<User>();
                    foreach (var it in importedList)
                    {
                        if (it == null) continue;
                        if (string.IsNullOrWhiteSpace(it.FullName)) continue;
                        if (string.IsNullOrWhiteSpace(it.Email)) continue;
                        cleaned.Add(it);
                    }

                    int countAdd = 0;
                    int countUpdate = 0;

                    foreach (var item in cleaned)
                    {
                        string excelEmail = item.Email.Trim();
                        int existingId = UserDAO.Instance.GetIdByEmail(excelEmail);

                        if (existingId != -1)
                        {
                            if (UserDAO.Instance.UpdateUser(existingId, item.FullName, item.Phone, item.Address, item.Gender, item.RoleName, item.HourlyWage))
                                countUpdate++;
                        }
                        else
                        {
                            if (UserDAO.Instance.InsertUser(item.FullName, excelEmail, item.Phone, item.Address, item.Gender, DataAccess.Constants.DEFAULT_PASSWORD, item.RoleName, item.HourlyWage))
                                countAdd++;
                        }
                    }

                    LoadData();
                    JetMoonMessageBox.Show($"Xử lý xong!\n- Thêm mới: {countAdd} nhân viên\n- Cập nhật thông tin: {countUpdate} nhân viên", "Kết quả", MsgType.Success);
                }
                catch (Exception ex)
                {
                    JetMoonMessageBox.Show("Lỗi: " + ex.Message, "Lỗi Import", MsgType.Error);
                }
            }
        }

        private bool ValidateImportedUsers(List<User> importedList, out string error)
        {
            error = string.Empty;

            if (importedList == null || importedList.Count == 0)
            {
                error = "Không tìm thấy dữ liệu trong file Excel. Vui lòng kiểm tra lại định dạng.\n" +
                        "Yêu cầu: file phải chứa các cột tương ứng với thuộc tính model: 'FullName', 'Email', 'RoleName'. (HourlyWage là tùy chọn nhưng phải >= 0 nếu có).";
                return false;
            }

            int total = importedList.Count;
            int missingFullName = 0;
            int missingEmail = 0;
            int missingRole = 0;
            int invalidEmailCount = 0;
            int negativeWageCount = 0;

            var emailSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dupEmails = new List<string>();

            foreach (var row in importedList)
            {
                if (row == null)
                {
                    missingFullName++;
                    missingEmail++;
                    missingRole++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.FullName)) missingFullName++;

                if (string.IsNullOrWhiteSpace(row.Email))
                {
                    missingEmail++;
                }
                else
                {
                    var email = row.Email.Trim();
                    // Simple email regex (avoid adding new using)
                    try
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                            invalidEmailCount++;
                    }
                    catch
                    {
                        invalidEmailCount++;
                    }

                    if (!emailSet.Add(email))
                    {
                        if (!dupEmails.Contains(email)) dupEmails.Add(email);
                    }
                }

                if (string.IsNullOrWhiteSpace(row.RoleName)) missingRole++;

                if (row.HourlyWage < 0) negativeWageCount++;
            }

            // Fail early on missing required columns
            if (missingFullName > 0)
            {
                error = $"File Excel không đúng định dạng: phát hiện {missingFullName} hàng thiếu 'FullName'.\nĐảm bảo file có cột 'FullName' và ô không rỗng.";
                return false;
            }

            if (missingEmail > 0)
            {
                error = $"File Excel không đúng định dạng: phát hiện {missingEmail} hàng thiếu 'Email'.\nĐảm bảo file có cột 'Email' và ô không rỗng.";
                return false;
            }

            if (missingRole > 0)
            {
                error = $"File Excel không đúng định dạng: phát hiện {missingRole} hàng thiếu 'RoleName'.\nĐảm bảo file có cột 'RoleName' và ô không rỗng.";
                return false;
            }

            // If majority of emails are invalid -> likely wrong mapping
            if (invalidEmailCount > total / 2)
            {
                error = "File Excel có vấn đề với cột 'Email' (nhiều giá trị không đúng định dạng). Vui lòng kiểm tra lại định dạng cột 'Email'.";
                return false;
            }

            // If majority of wages are negative -> likely wrong mapping
            if (negativeWageCount > total / 2)
            {
                error = "File Excel có vấn đề với cột 'HourlyWage' (nhiều giá trị âm). Vui lòng kiểm tra lại định dạng cột 'HourlyWage'.";
                return false;
            }

            if (dupEmails.Count > 0)
            {
                string sample;
                if (dupEmails.Count <= 5) sample = string.Join(", ", dupEmails);
                else
                {
                    var top5 = dupEmails.GetRange(0, 5);
                    sample = string.Join(", ", top5) + ", ...";
                }

                error = $"File Excel chứa email trùng lặp: {sample}\nVui lòng loại bỏ hoặc sửa các dòng trùng nhau trước khi import.";
                return false;
            }

            return true;
        }
        // --- Export ---
        private void ExportData()
        {
            var sfd = new SaveFileDialog() { Filter = "Excel|*.xlsx", FileName = "DS_NhanVien.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                var list = UserDAO.Instance.GetListUser();
                ExcelHelper.ExportList<User>(sfd.FileName, list, "NhanViens");
                JetMoonMessageBox.Show("Xuất xong!", "Thông báo", MsgType.Success);
            }
        }
    }
}