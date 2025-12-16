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
    /// Interaction logic for DateToDatePicker.xaml
    /// </summary>
    public partial class DateToDatePicker : UserControl
    {
        public DateToDatePicker()
        {
            InitializeComponent();
            LoadDatePicker(0);
        }


        private void Classify_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDate == null || EndDate == null) return;

            LoadDatePicker(Classify.SelectedIndex);
        }

        private void LoadDatePicker(int idx)
        {
            StartDate.LoadDate(idx);
            EndDate.LoadDate(idx);
        }
    }
}
