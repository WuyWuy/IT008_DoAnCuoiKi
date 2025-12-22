using QuanLyCaPhe.DAO;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Components; // [QUAN TRỌNG] Để dùng JetMoonMessageBox
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin.DetailWindow
{
    public partial class InputInfoDetailWindow : Window
    {
        private InputInfo _inputInfo;
        private double _originalCount = 0; // store original count to compute difference

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

                // Store original count for later diff calculation
                _originalCount = _inputInfo.Count;
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
                JetMoonMessageBox.Show("Vui lòng chọn nguyên liệu cần nhập!", "Thiếu thông tin", MsgType.Warning);
                return;
            }

            if (!double.TryParse(txtCount.Text, out double count) || count <= 0)
            {
                JetMoonMessageBox.Show("Số lượng nhập phải là số dương lớn hơn 0!", "Dữ liệu không hợp lệ", MsgType.Warning);
                txtCount.Focus();
                return;
            }

            if (!decimal.TryParse(txtInputPrice.Text, out decimal price) || price < 0)
            {
                JetMoonMessageBox.Show("Giá nhập không hợp lệ (Không được âm)!", "Dữ liệu không hợp lệ", MsgType.Warning);
                txtInputPrice.Focus();
                return;
            }

            if (dpDateInput.SelectedDate == null) dpDateInput.SelectedDate = DateTime.Now;

            // 2. Lấy dữ liệu
            int ingId = (int)cboIngredient.SelectedValue;
            DateTime dateInput = dpDateInput.SelectedDate.Value;

            try
            {
                bool isSuccess = false;

                if (_inputInfo == null) // TRƯỜNG HỢP THÊM MỚI
                {
                    if (InputInfoDAO.Instance.InsertInputInfo(ingId, dateInput, price, count))
                    {
                        JetMoonMessageBox.Show("Nhập kho thành công! Số lượng đã được cộng vào kho.", "Thành công", MsgType.Success);
                        isSuccess = true;
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Có lỗi xảy ra khi nhập kho. Vui lòng thử lại.", "Lỗi cơ sở dữ liệu", MsgType.Error);
                    }
                }
                else // TRƯỜNG HỢP SỬA
                {
                    // Call the new UpdateInputInfo method, passing original count to compute adjustment
                    if (InputInfoDAO.Instance.UpdateInputInfo(_inputInfo.Id, ingId, dateInput, price, count, _originalCount))
                    {
                        JetMoonMessageBox.Show("Cập nhật phiếu nhập thành công! Kho đã được điều chỉnh lại.", "Thành công", MsgType.Success);
                        isSuccess = true;
                    }
                    else
                    {
                        JetMoonMessageBox.Show("Cập nhật phiếu nhập thất bại.", "Lỗi cơ sở dữ liệu", MsgType.Error);
                    }
                }

                if (isSuccess)
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                JetMoonMessageBox.Show("Lỗi hệ thống: " + ex.Message, "Critical Error", MsgType.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}