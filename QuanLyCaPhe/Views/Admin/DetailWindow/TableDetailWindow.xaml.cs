using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models; // Check namespace của Table
using QuanLyCaPhe.Views.Components;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class TableDetailWindow : Window
    {
        private Table _table;

        public TableDetailWindow(Table table = null)
        {
            InitializeComponent();
            _table = table;

            if (_table != null)
            {
                txtName.Text = _table.Name;
                chkIsActive.IsChecked = _table.IsActive; // Load trạng thái
                txtNote.Text = _table.Note; // Load ghi chú

                foreach (ComboBoxItem item in cboStatus.Items)
                {
                    if (item.Content.ToString() == _table.Status) { cboStatus.SelectedItem = item; break; }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                JetMoonMessageBox.Show("Vui lòng nhập tên bàn!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            string name = txtName.Text.Trim();
            string status = (cboStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Free";
            bool isActive = chkIsActive.IsChecked ?? true; // Lấy giá trị checkbox
            string note = txtNote.Text.Trim(); // Lấy giá trị ghi chú

            try
            {
                bool success = false;
                if (_table == null) // Thêm mới
                {
                    success = TableDAO.Instance.InsertTable(name, status, isActive, note);
                }
                else // Cập nhật
                {
                    success = TableDAO.Instance.UpdateTable(_table.Id, name, status, isActive, note);
                }

                if (success)
                {
                    JetMoonMessageBox.Show("Lưu thông tin thành công!", "Hoàn tất", MsgType.Success);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    JetMoonMessageBox.Show("Lỗi CSDL!", "Lỗi", MsgType.Error);
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