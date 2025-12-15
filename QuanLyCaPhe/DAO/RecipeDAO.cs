using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyCaPhe.DAO
{
    public class RecipeDAO
    {
        private static RecipeDAO instance;
        public static RecipeDAO Instance
        {
            get { if (instance == null) instance = new RecipeDAO(); return instance; }
            private set { instance = value; }
        }
        private RecipeDAO() { }

        public List<Recipe> GetListRecipeByProID(int proId)
        {
            List<Recipe> list = new List<Recipe>();
            DataTable data = DBHelper.ExecuteQuery("SELECT * FROM Recipes WHERE ProId = " + proId);
            foreach (DataRow item in data.Rows) list.Add(new Recipe(item));
            return list;
        }

        public bool InsertRecipe(int proId, int ingId, double amount)
        {
            string query = "INSERT INTO Recipes (ProId, IngId, Amount) VALUES (@pid, @iid, @amt)";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@pid", proId),
                new SqlParameter("@iid", ingId),
                new SqlParameter("@amt", amount)
            }) > 0;
        }

        public bool DeleteRecipe(int id)
        {
            string query = "DELETE Recipes WHERE Id = " + id;
            return DBHelper.ExecuteNonQuery(query) > 0;
        }

        // Xóa tất cả công thức của 1 món (dùng khi reset công thức)
        public void DeleteRecipeByProID(int proId)
        {
            DBHelper.ExecuteNonQuery("DELETE Recipes WHERE ProId = " + proId);
        }
    }
}