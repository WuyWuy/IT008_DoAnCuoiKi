using System.Data;

namespace QuanLyCaPhe.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public int ProId { get; set; }
        public int IngId { get; set; }
        public double Amount { get; set; }

        public Recipe() { }

        public Recipe(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.ProId = (int)row["ProId"];
            this.IngId = (int)row["IngId"];
            this.Amount = Convert.ToDouble(row["Amount"]);
        }
    }
}