using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCaPhe.Models
{
    public class TopProduct
    {
        public string Name { get; set; } // Tên món
        public int Count { get; set; }   // Số lượng đã bán
        public int Rank { get; set; }    // Thứ hạng (1, 2, 3)
    }
}
