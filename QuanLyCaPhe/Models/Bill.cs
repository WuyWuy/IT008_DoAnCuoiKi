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

        // New
        public string PaymentMethod { get; set; } = string.Empty;
        public int? TimeUsedHours { get; set; }

        public Bill() { }

        public Bill(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.DateCheckIn = (DateTime)row["DateCheckIn"];

            var dateCheckOutTemp = row["DateCheckOut"];
            if (dateCheckOutTemp != DBNull.Value && dateCheckOutTemp.ToString() != "")
                this.DateCheckOut = (DateTime)dateCheckOutTemp;
            else
                this.DateCheckOut = null;

            this.Status = (int)row["Status"];
            this.Discount = (int)row["Discount"];
            this.TotalPrice = (decimal)row["TotalPrice"];
            this.TableId = (int)row["TableId"];

            this.TableName = row.Table.Columns.Contains("TableName") ? row["TableName"].ToString() : string.Empty;

            if (row.Table.Columns.Contains("UserId") && row["UserId"] != DBNull.Value)
            {
                this.UserId = (int)row["UserId"];
                this.StaffName = row.Table.Columns.Contains("StaffName") ? row["StaffName"].ToString() : string.Empty;
            }
            else
            {
                this.UserId = null;
                this.StaffName = string.Empty;
            }

            // Read new columns if present
            if (row.Table.Columns.Contains("PaymentMethod") && row["PaymentMethod"] != DBNull.Value)
            {
                this.PaymentMethod = row["PaymentMethod"].ToString();
            }
            else
            {
                this.PaymentMethod = string.Empty;
            }

            if (row.Table.Columns.Contains("TimeUsedHours") && row["TimeUsedHours"] != DBNull.Value)
            {
                this.TimeUsedHours = Convert.ToInt32(row["TimeUsedHours"]);
            }
            else
            {
                this.TimeUsedHours = null;
            }
        }
    }
}