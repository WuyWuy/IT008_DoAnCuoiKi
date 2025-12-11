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
    /// Interaction logic for IngredientsPage.xaml
    /// </summary>
    public partial class IngredientsPage : Page
    {
        public ObservableCollection<Ingredients> IngredientList { get; set; }

        public IngredientsPage()
        {
            InitializeComponent();
            Loaded += IngredientsPage_Loaded;
        }

        private void IngredientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDemoData();
        }

        private void LoadDemoData()
        {
            IngredientList = new ObservableCollection<Ingredients>
            {
                new Ingredients { Id = 1, IngName = "Cà phê Arabica", Unit = "kg", Quantity = 12.5f },
                new Ingredients { Id = 2, IngName = "Cà phê Robusta", Unit = "kg", Quantity = 8.2f },
                new Ingredients { Id = 3, IngName = "Sữa tươi", Unit = "lít", Quantity = 25.0f },
                new Ingredients { Id = 4, IngName = "Sữa đặc", Unit = "lon", Quantity = 15f },
                new Ingredients { Id = 5, IngName = "Đường trắng", Unit = "kg", Quantity = 32.7f },
                new Ingredients { Id = 6, IngName = "Đường nâu", Unit = "kg", Quantity = 10.5f },
                new Ingredients { Id = 7, IngName = "Trà Ô Long", Unit = "kg", Quantity = 5.8f },
                new Ingredients { Id = 8, IngName = "Trà đen", Unit = "kg", Quantity = 7.3f },
                new Ingredients { Id = 9, IngName = "Bột matcha", Unit = "kg", Quantity = 3.2f },
                new Ingredients { Id = 10, IngName = "Chocolate", Unit = "kg", Quantity = 6.5f },
                new Ingredients { Id = 11, IngName = "Hạt dẻ", Unit = "kg", Quantity = 4.1f },
                new Ingredients { Id = 12, IngName = "Vanilla", Unit = "lọ", Quantity = 8f },
                new Ingredients { Id = 13, IngName = "Caramel", Unit = "chai", Quantity = 12f },
                new Ingredients { Id = 14, IngName = "Cốc giấy", Unit = "cái", Quantity = 500f },
                new Ingredients { Id = 15, IngName = "Ống hút", Unit = "cái", Quantity = 1200f },
                new Ingredients { Id = 16, IngName = "Túi giấy", Unit = "cái", Quantity = 300f },
                new Ingredients { Id = 17, IngName = "Đá viên", Unit = "kg", Quantity = 150.0f },
                new Ingredients { Id = 18, IngName = "Kem tươi", Unit = "lít", Quantity = 8.5f },
                new Ingredients { Id = 19, IngName = "Siro dâu", Unit = "chai", Quantity = 6f },
                new Ingredients { Id = 20, IngName = "Siro vanilla", Unit = "chai", Quantity = 9f }
            };

            Ingedientsdg.ItemsSource = IngredientList;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
