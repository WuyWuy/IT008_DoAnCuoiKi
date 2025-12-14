using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCaPhe.Models
{
    public class Activity
    {
        public string Description { get; set; } // VD: "Thanh toán"
        public string Detail { get; set; }      // VD: "Bàn 5 - 150k"
        public string TimeAgo { get; set; }     // VD: "Vừa xong"
        public string IconPath { get; set; }    // Vẽ hình
        public string IconColor { get; set; }   // Màu icon
    }
}
