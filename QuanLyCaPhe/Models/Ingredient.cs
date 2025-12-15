using System.Data;

namespace QuanLyCaPhe.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string IngName { get; set; }
        public string Unit { get; set; }
        public double Quantity { get; set; } // SQL Float -> C# Double

        public Ingredient() { }

        public Ingredient(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.IngName = row["IngName"].ToString();
            this.Unit = row["Unit"].ToString();
            // Convert.ToDouble an toàn hơn ép kiểu trực tiếp với số thực
            this.Quantity = Convert.ToDouble(row["Quantity"]);
        }
    }
}