using Microsoft.Identity.Client;
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
    /// Interaction logic for SearchBar.xaml
    /// </summary>
    public partial class SearchBar : UserControl
    {
        public event EventHandler<string> Clicked;
        public string Text
        {
            get { return Searchtb.Text; }
            set { Searchtb.Text = value; }
        }

        public SearchBar()
        {
            InitializeComponent();
        }

        private void Searchbtn_Click(object sender, RoutedEventArgs e)
        {
            Clicked?.Invoke(this, Searchtb.Text);
        }

        private void Searchtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Clicked?.Invoke(this, Searchtb.Text);
        }
    }
}
