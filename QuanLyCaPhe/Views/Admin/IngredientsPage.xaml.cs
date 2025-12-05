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
        public ObservableCollection<Ingredient> IngredientList { get; set; }

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
            IngredientList = new ObservableCollection<Ingredient>
            {
                new Ingredient { Id = 1, IngName = "Cà phê Arabica", Unit = "kg", Quantity = 12.5 },
                new Ingredient { Id = 2, IngName = "Cà phê Robusta", Unit = "kg", Quantity = 8.2 },
                new Ingredient { Id = 3, IngName = "Sữa tươi", Unit = "lít", Quantity = 25.0 },
                new Ingredient { Id = 4, IngName = "Sữa đặc", Unit = "lon", Quantity = 15 },
                new Ingredient { Id = 5, IngName = "Đường trắng", Unit = "kg", Quantity = 32.7 },
                new Ingredient { Id = 6, IngName = "Đường nâu", Unit = "kg", Quantity = 10.5 },
                new Ingredient { Id = 7, IngName = "Trà Ô Long", Unit = "kg", Quantity = 5.8 },
                new Ingredient { Id = 8, IngName = "Trà đen", Unit = "kg", Quantity = 7.3 },
                new Ingredient { Id = 9, IngName = "Bột matcha", Unit = "kg", Quantity = 3.2 },
                new Ingredient { Id = 10, IngName = "Chocolate", Unit = "kg", Quantity = 6.5 },
                new Ingredient { Id = 11, IngName = "Hạt dẻ", Unit = "kg", Quantity = 4.1 },
                new Ingredient { Id = 12, IngName = "Vanilla", Unit = "lọ", Quantity = 8 },
                new Ingredient { Id = 13, IngName = "Caramel", Unit = "chai", Quantity = 12 },
                new Ingredient { Id = 14, IngName = "Cốc giấy", Unit = "cái", Quantity = 500 },
                new Ingredient { Id = 15, IngName = "Ống hút", Unit = "cái", Quantity = 1200 },
                new Ingredient { Id = 16, IngName = "Túi giấy", Unit = "cái", Quantity = 300 },
                new Ingredient { Id = 17, IngName = "Đá viên", Unit = "kg", Quantity = 150.0 },
                new Ingredient { Id = 18, IngName = "Kem tươi", Unit = "lít", Quantity = 8.5 },
                new Ingredient { Id = 19, IngName = "Siro dâu", Unit = "chai", Quantity = 6 },
                new Ingredient { Id = 20, IngName = "Siro vanilla", Unit = "chai", Quantity = 9 }
            };

            Productsdg.ItemsSource = IngredientList;
        }
    }
}
