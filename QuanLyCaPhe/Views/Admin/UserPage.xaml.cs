using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using QuanLyCaPhe.Helpers;
using Microsoft.Win32;
using QuanLyCaPhe.Views.Admin.DetailWindow;
using QuanLyCaPhe.Views.Components;
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

        // --- Sửa Món ---
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedUser = menuItem?.DataContext as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn món cần sửa!");
                return;
            }

            var f = new UserDetailWindow(selectedUser);
            if (f.ShowDialog() == true)
            {
                LoadData();
            }
        }

        // --- Xóa Món ---
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedUser = menuItem?.DataContext as User;

            if (selectedUser == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa nhân viên '{selectedUser.FullName}' không?",
                                         "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (UserDAO.Instance.DeleteUser(selectedUser.Id))
                {
                    MessageBox.Show("Đã xóa thành công!");
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Không thể xóa nhân viên này.",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            // pass hourly wage when updating
                            if (UserDAO.Instance.UpdateUser(existingId, item.FullName, item.Phone, item.Address, item.Gender, item.RoleName, item.HourlyWage))
                                countUpdate++;
                        }
                        else
                        {
                            // pass hourly wage when inserting
                            if (UserDAO.Instance.InsertUser(item.FullName, item.Email, item.Phone, item.Address, item.Gender, Constants.DEFAULT_PASSWORD, item.RoleName, item.HourlyWage))
                                countAdd++;
                        }
                    }

                    LoadData();
                    MessageBox.Show($"Xử lý xong!\n- Thêm mới: {countAdd} nhân viên\n- Cập nhật thông tin: {countUpdate} nhân viên", "Kết quả");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
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
                MessageBox.Show("Xuất xong!");
            }
        }
    }
}
