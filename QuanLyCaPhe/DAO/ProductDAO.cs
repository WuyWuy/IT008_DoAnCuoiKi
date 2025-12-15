using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;

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

        // --- Lấy Id từ tên ---
        public int GetIdByName(string name)
        {
            string query = "SELECT * FROM Products WHERE ProName = @name";

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

        // --- Lấy danh sách ---
        public List<Product> GetListProduct()
        {
            List<Product> list = new List<Product>();
            DataTable data = DBHelper.ExecuteQuery("SELECT * FROM Products");
            foreach (DataRow item in data.Rows) list.Add(new Product(item));
            return list;
        }

        // --- Tìm kiếm ---
        public List<Product> SearchProductByName(string name)
        {
            List<Product> list = new List<Product>();
            string query = "SELECT * FROM Products WHERE ProName LIKE @name";
            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@name", "%" + name + "%") });
            foreach (DataRow item in data.Rows) list.Add(new Product(item));
            return list;
        }
        // --- Đếm số dòng ---
        public int GetCountProduct()
        {
            string query = "SELECT COUNT(*) FROM Products";

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
            string query = "INSERT INTO Products (ProName, Price) VALUES (@name, @price)";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@price", price)
            }) > 0;
        }

        // --- Sửa món  ---
        public bool UpdateProduct(int id, string name, decimal price)
        {
            string query = "UPDATE Products SET ProName = @name, Price = @price WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@price", price),
                new SqlParameter("@id", id)
            }) > 0;
        }
        // --- Xóa món ---
        public bool DeleteProduct(int id)
        {
            string query = "DELETE Products WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@id", id)
            }) > 0;
        }
    }
}