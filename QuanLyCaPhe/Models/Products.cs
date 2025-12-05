using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCaPhe.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string? ProName { get; set; }
        public int Price { get; set; }
        public int CateId { get; set; }
    }
}
