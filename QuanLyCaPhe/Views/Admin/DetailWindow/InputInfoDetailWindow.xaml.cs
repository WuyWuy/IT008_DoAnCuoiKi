using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class InputInfoDetailWindow : Window
    {
        private InputInfo _inputInfo;

        // Constructor mặc định (Thêm mới)
        public InputInfoDetailWindow()
        {
            InitializeComponent();
            LoadIngredients();
            dpDateInput.SelectedDate = DateTime.Now; // Mặc định là hôm nay

            // Đăng ký sự kiện chọn combobox
            cboIngredient.SelectionChanged += CboIngredient_SelectionChanged;
        }

        // Constructor cho Edit (Xem/Sửa)
        public InputInfoDetailWindow(InputInfo info) : this()
        {
            _inputInfo = info;
            if (_inputInfo != null)
            {
                cboIngredient.SelectedValue = _inputInfo.IngId;
                dpDateInput.SelectedDate = _inputInfo.DateInput;
                txtCount.Text = _inputInfo.Count.ToString();
                txtInputPrice.Text = _inputInfo.InputPrice.ToString("G29"); // Format số nguyên

                // Khóa ComboBox lại nếu đang sửa (để tránh sai lệch kho nghiêm trọng)
                cboIngredient.IsEnabled = false;
            }
        }

        private void LoadIngredients()
        {
            // Lấy danh sách nguyên liệu từ DAO
            List<Ingredient> list = IngredientDAO.Instance.GetListIngredient();
            cboIngredient.ItemsSource = list;
        }

        private void CboIngredient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Khi chọn nguyên liệu -> Hiển thị đơn vị tính tương ứng
            if (cboIngredient.SelectedItem is Ingredient selected)
            {
                txtUnit.Text = selected.Unit;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (cboIngredient.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn nguyên liệu cần nhập!");
                return;
            }

            if (!double.TryParse(txtCount.Text, out double count) || count <= 0)
            {
                MessageBox.Show("Số lượng nhập phải là số dương!");
                return;
            }

            if (!decimal.TryParse(txtInputPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Giá nhập không hợp lệ!");
                return;
            }

            if (dpDateInput.SelectedDate == null) dpDateInput.SelectedDate = DateTime.Now;

            // 2. Lấy dữ liệu
            int ingId = (int)cboIngredient.SelectedValue;
            DateTime dateInput = dpDateInput.SelectedDate.Value;

            try
            {
                if (_inputInfo == null) // TRƯỜNG HỢP THÊM MỚI
                {
                    if (InputInfoDAO.Instance.InsertInputInfo(ingId, dateInput, price, count))
                    {
                        MessageBox.Show("Nhập kho thành công! Số lượng đã được cộng vào kho.");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Có lỗi khi nhập kho.");
                    }
                }
                else // TRƯỜNG HỢP SỬA (Lưu ý: Logic sửa kho khá phức tạp)
                {
                    MessageBox.Show("Chức năng cập nhật phiếu nhập đang được bảo trì để đảm bảo an toàn kho.\nVui lòng xóa phiếu cũ và nhập lại nếu cần.", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}