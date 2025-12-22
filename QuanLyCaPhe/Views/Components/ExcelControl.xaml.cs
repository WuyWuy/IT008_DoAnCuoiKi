using System;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Components
{
    public partial class ExcelControl : UserControl
    {
        public event EventHandler<string> Clicked;

        public ExcelControl()
        {
            InitializeComponent();
        }

        // --- 1. Thuộc tính cho nút IMPORT ---
        public static readonly DependencyProperty IsImportEnabledProperty =
            DependencyProperty.Register(nameof(IsImportEnabled), typeof(bool), typeof(ExcelControl), new PropertyMetadata(true));

        public bool IsImportEnabled
        {
            get { return (bool)GetValue(IsImportEnabledProperty); }
            set { SetValue(IsImportEnabledProperty, value); }
        }

        // --- 2. Thuộc tính cho nút EXPORT ---
        public static readonly DependencyProperty IsExportEnabledProperty =
            DependencyProperty.Register(nameof(IsExportEnabled), typeof(bool), typeof(ExcelControl), new PropertyMetadata(true));

        public bool IsExportEnabled
        {
            get { return (bool)GetValue(IsExportEnabledProperty); }
            set { SetValue(IsExportEnabledProperty, value); }
        }

        // --- Xử lý sự kiện click ---
        private void Importbtn_Click(object sender, RoutedEventArgs e)
        {
            Clicked?.Invoke(this, "Import");
        }

        private void Exportbtn_Click(object sender, RoutedEventArgs e)
        {
            Clicked?.Invoke(this, "Export");
        }
    }
}