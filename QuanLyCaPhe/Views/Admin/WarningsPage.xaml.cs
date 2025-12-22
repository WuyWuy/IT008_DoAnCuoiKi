using QuanLyCaPhe.Services;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class WarningsPage : Page
    {
        public ObservableCollection<string> WarningList { get; set; }

        // Show fewer than200 warnings in the UI (max199)
        private const int MaxWarningsToShow =199;

        public WarningsPage()
        {
            InitializeComponent();
            WarningList = new ObservableCollection<string>();
            lbWarnings.ItemsSource = WarningList;

            // Load lần đầu
            GlobalService.CheckWarnings();
            UpdateList();

            // Đăng ký lắng nghe sự kiện
            GlobalService.OnWarningUpdated += () => { Dispatcher.Invoke(UpdateList); };
        }

        private void UpdateList()
        {
            WarningList.Clear();

            int added =0;
            foreach (var w in GlobalService.CurrentWarnings)
            {
                if (added >= MaxWarningsToShow)
                    break;

                WarningList.Add(w);
                added++;
            }

            // If there are more warnings than we display, append '+' to indicate more exist
            txtCount.Text = WarningList.Count.ToString();
            if (GlobalService.CurrentWarnings.Count > MaxWarningsToShow)
            {
                txtCount.Text += "+";
            }

            // Hiện text trống nếu không có lỗi
            txtEmpty.Visibility = WarningList.Count ==0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
    }
}