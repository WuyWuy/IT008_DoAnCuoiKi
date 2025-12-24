using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models; // Hoặc namespace QuanLyCaPhe chứa Table.cs
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace QuanLyCaPhe.DAO
{
    public class TableDAO
    {
        private static TableDAO instance;
        public static TableDAO Instance
        {
            get { if (instance == null) instance = new TableDAO(); return instance; }
            private set { instance = value; }
        }
        private TableDAO() { }

        // Load danh sách bàn
        public List<Table> LoadTableList()
        {
            List<Table> tableList = new List<Table>();
            // Lấy tất cả bàn (cả Active và Inactive) để Admin quản lý
            DataTable data = DBHelper.ExecuteQuery("SELECT * FROM TableCoffees");
            foreach (DataRow item in data.Rows) tableList.Add(new Table(item));
            return tableList;
        }

        // Cập nhật Insert
        public bool InsertTable(string name, string status, bool isActive, string note)
        {
            string query = "INSERT INTO TableCoffees (TableName, Status, IsActive, Note) VALUES (@name, @status, @active, @note)";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
            new SqlParameter("@name", name),
            new SqlParameter("@status", status),
            new SqlParameter("@active", isActive),
            new SqlParameter("@note", note ?? "") // Nếu null thì lưu rỗng
        }) > 0;
        }

        // Cập nhật Update
        public bool UpdateTable(int id, string name, string status, bool isActive, string note)
        {
            string query = "UPDATE TableCoffees SET TableName = @name, Status = @status, IsActive = @active, Note = @note WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
            new SqlParameter("@name", name),
            new SqlParameter("@status", status),
            new SqlParameter("@active", isActive),
            new SqlParameter("@note", note ?? ""),
            new SqlParameter("@id", id)
        }) > 0;
        }

        public bool DeleteTable(int id)
        {
            string query = "DELETE FROM TableCoffees WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@id", id) }) > 0;
        }
    }
}