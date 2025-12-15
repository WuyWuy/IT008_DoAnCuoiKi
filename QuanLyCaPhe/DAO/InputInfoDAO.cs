using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient; // Dùng cho SqlParameter
using System.Collections.Generic;

namespace QuanLyCaPhe.DAO
{
    public class InputInfoDAO
    {
        private static InputInfoDAO instance;
        public static InputInfoDAO Instance
        {
            get { if (instance == null) instance = new InputInfoDAO(); return instance; }
            private set { instance = value; }
        }
        private InputInfoDAO() { }

        // --- 1. LẤY DANH SÁCH ---
        public List<InputInfo> GetListInputInfo()
        {
            List<InputInfo> list = new List<InputInfo>();
            // JOIN để lấy tên hiển thị
            string query = @"
                SELECT i.*, ing.IngName AS IngredientName 
                FROM InputInfos i
                JOIN Ingredients ing ON i.IngId = ing.Id
                ORDER BY i.DateInput DESC";

            DataTable data = DBHelper.ExecuteQuery(query);
            foreach (DataRow item in data.Rows) list.Add(new InputInfo(item));
            return list;
        }

        // --- 2. TÌM KIẾM ---
        public List<InputInfo> SearchInputInfo(string keyword)
        {
            List<InputInfo> list = new List<InputInfo>();
            string query = @"
                SELECT i.*, ing.IngName AS IngredientName 
                FROM InputInfos i
                JOIN Ingredients ing ON i.IngId = ing.Id
                WHERE ing.IngName LIKE @key
                ORDER BY i.DateInput DESC";

            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] {
                new SqlParameter("@key", "%" + keyword + "%")
            });
            foreach (DataRow item in data.Rows) list.Add(new InputInfo(item));
            return list;
        }

        // --- 3. NHẬP KHO ---
        public bool InsertInputInfo(int ingId, DateTime dateInput, decimal price, double count)
        {
            // Bước 1: Thêm phiếu nhập bằng DBHelper
            string query = "INSERT INTO InputInfos (IngId, DateInput, InputPrice, Count) VALUES (@iid, @date, @price, @count)";

            int result = DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@iid", ingId),
                new SqlParameter("@date", dateInput), // Lưu ý: Truyền ngày nhập từ tham số, không dùng GETDATE() cứng
                new SqlParameter("@price", price),
                new SqlParameter("@count", count)
            });

            if (result > 0)
            {
                // Bước 2: Gọi IngredientDAO để cộng tồn kho (Tái sử dụng code, đỡ viết lại SQL)
                IngredientDAO.Instance.UpdateQuantity(ingId, count);
                return true;
            }
            return false;
        }

        // --- 4. XÓA PHIẾU NHẬP ---
        public bool DeleteInputInfo(int id, int ingId, double countToDelete)
        {
            // Bước 1: Xóa phiếu nhập trong DB
            string query = "DELETE FROM InputInfos WHERE Id = @id";

            int result = DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@id", id)
            });

            if (result > 0)
            {
                // Bước 2: Gọi IngredientDAO để TRỪ tồn kho (Truyền số âm để trừ)
                IngredientDAO.Instance.UpdateQuantity(ingId, -countToDelete);
                return true;
            }
            return false;
        }
    }
}