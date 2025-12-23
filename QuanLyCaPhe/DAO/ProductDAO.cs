using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace QuanLyCaPhe.DAO
{
    public class ProductDAO
    {
        private static ProductDAO instance;
        public static ProductDAO Instance
        {
            get { if (instance == null) instance = new ProductDAO(); return instance; }
            private set { instance = value; }
        }
        private ProductDAO() { }

        // --- Lấy Id từ tên (Chỉ tìm món đang Active) ---
        public int GetIdByName(string name)
        {
            // [CẬP NHẬT] Thêm điều kiện IsActive = 1
            string query = "SELECT * FROM Products WHERE ProName = @name AND IsActive = 1";

            System.Data.DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[]
            {
                new SqlParameter("@name", name)
            });

            if (data.Rows.Count > 0)
            {
                Product p = new Product(data.Rows[0]);
                return p.Id;
            }
            return -1;
        }

        // --- Lấy danh sách (Chỉ lấy món đang Active) ---
        public List<Product> GetListProduct()
        {
            List<Product> list = new List<Product>();

            // [CẬP NHẬT] Thêm WHERE IsActive = 1
            string query = "SELECT * FROM Products WHERE IsActive = 1";

            DataTable data = DBHelper.ExecuteQuery(query);
            foreach (DataRow item in data.Rows) list.Add(new Product(item));
            return list;
        }

        // --- Tìm kiếm (Chỉ tìm món đang Active) ---
        public List<Product> SearchProductByName(string name)
        {
            List<Product> list = new List<Product>();

            // [CẬP NHẬT] Thêm AND IsActive = 1
            string query = "SELECT * FROM Products WHERE ProName LIKE @name AND IsActive = 1";

            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@name", "%" + name + "%") });
            foreach (DataRow item in data.Rows) list.Add(new Product(item));
            return list;
        }

        // --- Đếm số dòng (Chỉ đếm món đang Active) ---
        public int GetCountProduct()
        {
            // [CẬP NHẬT] Thêm WHERE IsActive = 1
            string query = "SELECT COUNT(*) FROM Products WHERE IsActive = 1";

            try
            {
                return Convert.ToInt32(DBHelper.ExecuteScalar(query));
            }
            catch
            {
                return 0;
            }
        }

        // --- Thêm món ---
        public bool InsertProduct(string name, decimal price)
        {
            // [CẬP NHẬT] Thêm cột IsActive với giá trị mặc định là 1 (True)
            string query = "INSERT INTO Products (ProName, Price, IsActive) VALUES (@name, @price, 1)";

            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@price", price)
            }) > 0;
        }

        // --- Sửa món ---
        public bool UpdateProduct(int id, string name, decimal price)
        {
            string query = "UPDATE Products SET ProName = @name, Price = @price WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@price", price),
                new SqlParameter("@id", id)
            }) > 0;
        }

        // --- Xóa món (Soft Delete) ---
        public bool DeleteProduct(int id)
        {
            // [CẬP NHẬT] Thay lệnh DELETE bằng UPDATE IsActive
            string query = "UPDATE Products SET IsActive = 0 WHERE Id = @id";

            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@id", id)
            }) > 0;
        }
    }
}