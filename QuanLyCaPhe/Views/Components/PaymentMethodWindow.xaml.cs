using System;
using System.Windows;

namespace QuanLyCaPhe.Views.Components
{
    /// <summary>
    /// Interaction logic for PaymentMethodWindow.xaml
    /// </summary>
    public partial class PaymentMethodWindow : Window
    {
        public string SelectedMethod { get; private set; }

        public PaymentMethodWindow()
        {
            InitializeComponent();
            SelectedMethod = string.Empty;
        }

        private void BtnCash_Click(object sender, RoutedEventArgs e)
        {
            SelectedMethod = "Tiền mặt";
            this.DialogResult = true;
            this.Close();
        }

        private void BtnTransfer_Click(object sender, RoutedEventArgs e)
        {
            SelectedMethod = "Chuyển khoản";
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
