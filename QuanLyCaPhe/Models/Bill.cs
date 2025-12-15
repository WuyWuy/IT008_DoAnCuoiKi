using System;
using System.Data;

namespace QuanLyCaPhe.Models
{
    public class Bill
    {
        public string TableName { get; set; }
        public string StaffName { get; set; }

        public int Id { get; set; }
        public DateTime DateCheckIn { get; set; }
        public DateTime? DateCheckOut { get; set; } // Dùng ? cho phép Null
        public int Status { get; set; }
        public int Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public int TableId { get; set; }
        public int? UserId { get; set; } // Dùng ? cho phép Null

        public Bill() { }

        public Bill(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.DateCheckIn = (DateTime)row["DateCheckIn"];

            // Kiểm tra Null cho DateCheckOut
            var dateCheckOutTemp = row["DateCheckOut"];
            if (dateCheckOutTemp.ToString() != "")
                this.DateCheckOut = (DateTime)dateCheckOutTemp;
            else
                this.DateCheckOut = null;

            this.Status = (int)row["Status"];
            this.Discount = (int)row["Discount"];
            this.TotalPrice = (decimal)row["TotalPrice"];
            this.TableId = (int)row["TableId"];
            this.TableName = row["TableName"].ToString();
            // Kiểm tra Null cho UserId (trường hợp chưa có ai thanh toán hoặc user bị xóa)
            if (row["UserId"] != DBNull.Value)
            {
                this.UserId = (int)row["UserId"];
                this.StaffName = row["StaffName"].ToString();
            }    
            else
            {
                this.UserId = null;
                this.StaffName = null;
            }   
        }
    }
}