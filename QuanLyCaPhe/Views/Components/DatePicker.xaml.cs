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
    /// Interaction logic for DatePicker.xaml
    /// </summary>
    public partial class DatePicker : UserControl
    {
        public DatePicker()
        {
            InitializeComponent();
            LoadDate(0);
        }

        public void LoadDate(int idx = 0)
        {
            if (cbDay == null || cbMonth == null || cbYear == null) return;

            cbDay.IsEnabled = false;
            cbMonth.IsEnabled = false;
            cbYear.IsEnabled = false;

            if (idx <= 0) cbDay.IsEnabled = true;
            if (idx <= 1) cbMonth.IsEnabled = true;
            if (idx <= 2) cbYear.IsEnabled = true;

        }
    }
}
