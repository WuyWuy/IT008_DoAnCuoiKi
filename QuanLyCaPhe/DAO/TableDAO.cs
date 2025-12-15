using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyCaPhe.DAO
{
    public class TableDAO
    {
        public static int TableWidth = 90;
        public static int TableHeight = 90;
        private static TableDAO instance;
        public static TableDAO Instance
        {
            get { if (instance == null) instance = new TableDAO(); return instance; }
            private set { instance = value; }
        }
        private TableDAO() { }

        public List<Models.Table> LoadTableList()
        {
            List<Models.Table> tableList = new List<Models.Table>();
            DataTable data = DBHelper.ExecuteQuery("SELECT * FROM TableCoffees");
            foreach (DataRow item in data.Rows) tableList.Add(new Models.Table(item));
            return tableList;
        }

        public bool InsertTable(string name, string status)
        {
            string query = "INSERT INTO TableCoffees (TableName, Status) VALUES (@name, @status)";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@status", status)
            }) > 0;
        }

        public bool UpdateTable(int id, string name, string status)
        {
            string query = "UPDATE TableCoffees SET TableName = @name, Status = @status WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@status", status),
                new SqlParameter("@id", id)
            }) > 0;
        }

        public bool DeleteTable(int id)
        {
            string query = "DELETE TableCoffees WHERE Id = " + id;
            return DBHelper.ExecuteNonQuery(query) > 0;
        }
    }
}