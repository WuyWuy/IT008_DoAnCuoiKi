using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCaPhe.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string IngName { get; set; }
        public string Unit { get; set; }
        public double Quantity { get; set; }
    }
}
