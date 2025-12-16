using System.Data;

namespace QuanLyCaPhe.Models
{
    public class BillInfo
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public int ProId { get; set; }
        public int Count { get; set; }

        public BillInfo() { }

        public BillInfo(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.BillId = (int)row["BillId"];
            this.ProId = (int)row["ProId"];
            this.Count = (int)row["Count"];
        }
    }
}