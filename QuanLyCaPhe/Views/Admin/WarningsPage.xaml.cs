using QuanLyCaPhe.Services;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace QuanLyCaPhe.Views.Admin
{
    public partial class WarningsPage : Page
    {
        public ObservableCollection<string> WarningList { get; set; }

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
            foreach (var w in GlobalService.CurrentWarnings)
            {
                WarningList.Add(w);
            }
            txtCount.Text = WarningList.Count.ToString();

            // Hiện text trống nếu không có lỗi
            txtEmpty.Visibility = WarningList.Count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
    }
}