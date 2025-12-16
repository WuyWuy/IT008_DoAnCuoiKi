using System;
using System.Data;

namespace QuanLyCaPhe.Models
{
    public class InputInfo
    {
        public int Id { get; set; }
        public int IngId { get; set; }
        public string IngredientName { get; set; } // Thuộc tính phụ để hiển thị tên
        public string Unit { get; set; } // đơn vị
        public DateTime DateInput { get; set; }
        public decimal InputPrice { get; set; } // Tổng tiền lô hàng
        public double Count { get; set; }      // Số lượng nhập

        public InputInfo() { }

        public InputInfo(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.IngId = (int)row["IngId"];
            this.DateInput = (DateTime)row["DateInput"];
            this.InputPrice = Convert.ToDecimal(row["InputPrice"]);
            this.Count = Convert.ToDouble(row["Count"]);

            // Kiểm tra nếu có cột IngredientName (do lệnh JOIN tạo ra) thì mới gán
            if (row.Table.Columns.Contains("IngredientName"))
            {
                this.IngredientName = row["IngredientName"].ToString();
            }

            // Kiểm tra nếu có cột Unit thì gán
            if (row.Table.Columns.Contains("Unit"))
            {
                this.Unit = row["Unit"] != DBNull.Value ? row["Unit"].ToString() : string.Empty;
            }
        }

        // Helper hiển thị số lượng kèm đơn vị
        public string DisplayQuantity
        {
            get
            {
                string qty;
                if (Math.Abs(Count - Math.Round(Count)) <0.0001)
                    qty = ((long)Math.Round(Count)).ToString();
                else
                    qty = Count.ToString("G");

                return string.IsNullOrEmpty(Unit) ? qty : $"{qty} {Unit}";
            }
        }
    }
}