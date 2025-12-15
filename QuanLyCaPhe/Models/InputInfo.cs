using System;
using System.Data;

namespace QuanLyCaPhe.Models
{
    public class InputInfo
    {
        public int Id { get; set; }
        public int IngId { get; set; }
        public string IngredientName { get; set; } // Thuộc tính phụ để hiển thị tên
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
        }
    }
}