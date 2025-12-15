using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyCaPhe.DAO
{
    public class IngredientDAO
    {
        private static IngredientDAO instance;
        public static IngredientDAO Instance
        {
            get { if (instance == null) instance = new IngredientDAO(); return instance; }
            private set { instance = value; }
        }
        private IngredientDAO() { }


        // =========================== SEARCH - IMPORT - EXPORT =================================
        // --- Lấy Id từ tên ---
        public int GetIdByName(string name)
        {
            string query = "SELECT * FROM Ingredients WHERE IngName = @name";

            System.Data.DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[]
            {
                new SqlParameter("@name", name)
            });

            if (data.Rows.Count > 0)
            {
                Ingredient p = new Ingredient(data.Rows[0]);
                return p.Id;
            }
            return -1;
        }

        // --- Lấy danh sách ---
        public List<Ingredient> GetListIngredient()
        {
            List<Ingredient> list = new List<Ingredient>();
            DataTable data = DBHelper.ExecuteQuery("SELECT * FROM Ingredients");
            foreach (DataRow item in data.Rows) list.Add(new Ingredient(item));
            return list;
        }

        // --- Tìm kiếm ---
        public List<Ingredient> SearchIngredientByName(string name)
        {
            List<Ingredient> list = new List<Ingredient>();
            string query = "SELECT * FROM Ingredients WHERE IngName LIKE @name";
            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@name", "%" + name + "%") });
            foreach (DataRow item in data.Rows) list.Add(new Ingredient(item));
            return list;
        }
        // --- Đếm số dòng ---
        public int GetCountIngradient()
        {
            string query = "SELECT COUNT(*) FROM Ingredients";

            try
            {
                return Convert.ToInt32(DBHelper.ExecuteScalar(query));
            }
            catch
            {
                return 0;
            }
        }

        // =========================== CHỨC NĂNG CƠ BẢN =================================
        // --- Thêm nguyên liệu ---
        public bool InsertIngredient(string name, string unit, double quantity)
        {
            string query = "INSERT INTO Ingredients (IngName, Unit, Quantity) VALUES (@name, @unit, @quan)";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@unit", unit),
                new SqlParameter("@quan", quantity)
            }) > 0;
        }
        // --- Sửa nguyên liệu ---
        public bool UpdateIngredient(int id, string name, string unit, double quantity)
        {
            string query = "UPDATE Ingredients SET IngName = @name, Unit = @unit, Quantity = @quan WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@unit", unit),
                new SqlParameter("@quan", quantity),
                new SqlParameter("@id", id)
            }) > 0;
        }

        // --- Cập nhật tồn kho ---
        public void UpdateQuantity(int id, double amount)
        {
            string query = "UPDATE Ingredients SET Quantity = Quantity + @amount WHERE Id = @id";
            DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@amount", amount),
                new SqlParameter("@id", id)
            });
        }
        // --- Xóa nguyên liệu ---
        public bool DeleteIngredient(int id)
        {
            string query = "DELETE Ingredients WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@id", id)
            }) > 0;
        }
    }
}