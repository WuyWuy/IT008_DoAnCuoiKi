using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCaPhe.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? ProName { get; set; }
        public decimal Price { get; set; }

        // Constructor mặc định
        public Product() { }

        // Constructor nhận DataRow để tự chuyển đổi (Đây là chỗ hiểu bản chất nè)
        public Product(System.Data.DataRow row)
        {
            this.Id = (int)row["Id"];
            this.ProName = row["ProName"].ToString();
            this.Price = (decimal)row["Price"]; // Lưu ý ép kiểu đúng với SQL
        }
    }
}
