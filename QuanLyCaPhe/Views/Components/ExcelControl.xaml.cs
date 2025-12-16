using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyCaPhe.Views.Components
{
    /// <summary>
    /// Interaction logic for ExcelControl.xaml
    /// </summary>
    public partial class ExcelControl : UserControl
    {
        public event EventHandler<string> Clicked;
        public ExcelControl()
        {
            InitializeComponent();
        }

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
