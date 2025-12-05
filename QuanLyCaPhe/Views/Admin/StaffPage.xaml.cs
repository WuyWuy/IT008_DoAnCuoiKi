using QuanLyCaPhe.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace QuanLyCaPhe.Views.Admin
{
    /// <summary>
    /// Interaction logic for StaffPage.xaml
    /// </summary>
    public partial class StaffPage : Page
    {
        public ObservableCollection<Users> UserList { get; set; }

        public StaffPage()
        {
            InitializeComponent();
            Loaded += StaffPage_Loaded;
        }

        private void StaffPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDemoData();
        }

        private void LoadDemoData()
        {
            UserList = new ObservableCollection<Users>
            {
                new Users { Id = 1, FullName = "Nguyễn Văn A", Address = "123 Đường ABC, Quận 1, TP.HCM",
                           Phone = "0901234567", Email = "nguyenvana@gmail.com", RoleName = "Quản lý" },
                new Users { Id = 2, FullName = "Trần Thị B", Address = "456 Đường XYZ, Quận 3, TP.HCM",
                           Phone = "0912345678", Email = "tranthib@gmail.com", RoleName = "Nhân viên pha chế" },
                new Users { Id = 3, FullName = "Lê Văn C", Address = "789 Đường DEF, Quận 5, TP.HCM",
                           Phone = "0923456789", Email = "levanc@gmail.com", RoleName = "Thu ngân" },
                new Users { Id = 4, FullName = "Phạm Thị D", Address = "321 Đường GHI, Quận 10, TP.HCM",
                           Phone = "0934567890", Email = "phamthid@gmail.com", RoleName = "Phục vụ" },
                new Users { Id = 5, FullName = "Hoàng Văn E", Address = "654 Đường JKL, Quận Bình Thạnh, TP.HCM",
                           Phone = "0945678901", Email = "hoangvane@gmail.com", RoleName = "Quản lý kho" },
                new Users { Id = 6, FullName = "Đỗ Thị F", Address = "987 Đường MNO, Quận Tân Bình, TP.HCM",
                           Phone = "0956789012", Email = "dothif@gmail.com", RoleName = "Nhân viên pha chế" },
                new Users { Id = 7, FullName = "Vũ Văn G", Address = "147 Đường PQR, Quận Phú Nhuận, TP.HCM",
                           Phone = "0967890123", Email = "vuvang@gmail.com", RoleName = "Phục vụ" },
                new Users { Id = 8, FullName = "Bùi Thị H", Address = "258 Đường STU, Quận Gò Vấp, TP.HCM",
                           Phone = "0978901234", Email = "buithih@gmail.com", RoleName = "Thu ngân" }
            };

            Staffsdg.ItemsSource = UserList;
        }
    }
}
