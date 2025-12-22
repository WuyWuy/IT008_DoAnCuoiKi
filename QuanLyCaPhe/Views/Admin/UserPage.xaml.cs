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

                    int countAdd = 0;
                    int countUpdate = 0;

                    foreach (var item in importedList)
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
                            if (UserDAO.Instance.InsertUser(item.FullName, item.Email, item.Phone, item.Address, item.Gender, Constants.DEFAULT_PASSWORD, item.RoleName, item.HourlyWage))
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