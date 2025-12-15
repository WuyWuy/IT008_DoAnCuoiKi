using System.Data;

namespace QuanLyCaPhe.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string Status { get; set; }

        public Table() { }

        public Table(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.TableName = row["TableName"].ToString();
            this.Status = row["Status"].ToString();
        }
    }
}