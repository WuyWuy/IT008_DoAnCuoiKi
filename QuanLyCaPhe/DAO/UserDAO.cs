using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;

namespace QuanLyCaPhe.DAO
{
    public class UserDAO
    {
        private static UserDAO instance;
        public static UserDAO Instance
        {
            get { if (instance == null) instance = new UserDAO(); return instance; }
            private set { instance = value; }
        }
        private UserDAO() { }

        // --- CÁC HÀM BẢO MẬT  ---

        // --- Tạo Hash và Salt ---
        private (string hashBase64, string saltBase64) GeneratePasswordHash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            // Băm mật khẩu 100.000 lần (Chống hack cực tốt)
            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        // --- Kiểm tra mật khẩu ---
        private bool VerifyPassword(string password, string storedHashBase64, string storedSaltBase64)
        {
            if (string.IsNullOrEmpty(storedHashBase64) || string.IsNullOrEmpty(storedSaltBase64))
                return false;

            var salt = Convert.FromBase64String(storedSaltBase64);
            var storedHash = Convert.FromBase64String(storedHashBase64);

            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);

            // So sánh an toàn 
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }

        // --- CÁC CHỨC NĂNG NGHIỆP VỤ ---

        // --- Đăng nhập ---
        public bool Login(string email, string rawPassword)
        {
            string query = "SELECT PasswordHash, PasswordSalt, IsActive FROM Users WHERE Email = @email";
            DataTable result = DBHelper.ExecuteQuery(query, new SqlParameter[] {
                new SqlParameter("@email", email)
            });

            if (result.Rows.Count == 0) return false;
            if (Convert.ToBoolean(result.Rows[0]["IsActive"]) == false) return false;

            string storedHash = result.Rows[0]["PasswordHash"].ToString();
            string storedSalt = result.Rows[0]["PasswordSalt"].ToString();

            return VerifyPassword(rawPassword, storedHash, storedSalt);
        }

        // --- Đăng ký ---
        public bool InsertUser(string name, string email, string phone, string address, string gender, string rawPassword, string role)
        {
            // Tự động mã hóa mật khẩu trước khi lưu
            var (hash, salt) = GeneratePasswordHash(rawPassword);

            string query = "INSERT INTO Users (FullName, Email, Phone, Address, Gender, PasswordHash, PasswordSalt, RoleName, IsActive) " +
                           "VALUES (@name, @email, @phone, @address, @gender, @hash, @salt, @role, 1)";

            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@email", email),
                new SqlParameter("@phone", phone),
                new SqlParameter("@address", address),
                new SqlParameter("@gender", gender),
                new SqlParameter("@hash", hash), 
                new SqlParameter("@salt", salt), 
                new SqlParameter("@role", role)
            }) > 0;
        }

        // --- Đổi mật khẩu ---
        public bool UpdatePassword(int id, string newRawPassword)
        {
            var (newHash, newSalt) = GeneratePasswordHash(newRawPassword);

            string query = "UPDATE Users SET PasswordHash = @pass, PasswordSalt = @salt WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@pass", newHash),
                new SqlParameter("@salt", newSalt),
                new SqlParameter("@id", id)
            }) > 0;
        }

        // --- Lấy Id từ Email ---

        public int GetIdByEmail(string email)
        {
            string query = "SELECT * FROM Users WHERE Email = @email";
            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@email", email) });
            if (data.Rows.Count > 0) return (int)data.Rows[0]["Id"];
            return -1;
        }

        // --- Lấy Role của User ---
        public string GetUserRole(int id)
        {
            string query = "SELECT RoleName FROM Users WHERE Id = @id";
            object result = DBHelper.ExecuteScalar(query, new SqlParameter[] { new SqlParameter("@id", id) });
            return result != null ? result.ToString() : "Staff";
        }

        // --- Lấy danh sách ---
        public List<User> GetListUser()
        {
            List<User> list = new List<User>();
            DataTable data = DBHelper.ExecuteQuery("SELECT * FROM Users WHERE IsActive = 1"); 
            foreach (DataRow item in data.Rows) list.Add(new User(item));
            return list;
        }

        // --- Tìm kiếm ---
        public List<User> SearchUserByAll (string str)
        {
            List<User> list = new List<User>();
            string query = "SELECT * FROM Users WHERE (FullName LIKE @str OR Email LIKE @str OR RoleName LIKE @str) AND IsActive = 1";
            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@str", "%" + str + "%") });
            foreach (DataRow item in data.Rows) list.Add(new User(item));
            return list;
        }

        // --- Đếm số dòng ---
        public int GetCountUser()
        {
            string query = "SELECT COUNT(*) FROM User";

            try
            {
                return Convert.ToInt32(DBHelper.ExecuteScalar(query));
            }
            catch
            {
                return 0;
            }
        }

        // --- Sửa ---
        public bool UpdateUser(int id, string name, string phone, string address, string gender, string role)
        {
            string query = "UPDATE Users SET FullName = @name, Phone = @phone, Address = @addr, Gender = @gender, RoleName = @role WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", name),
                new SqlParameter("@phone", phone),
                new SqlParameter("@addr", address),
                new SqlParameter("@gender", gender),
                new SqlParameter("@role", role),
                new SqlParameter("@id", id)
            }) > 0;
        }

        // --- Xóa ---
        public bool DeleteUser(int id)
        {
            string query = "UPDATE Users SET IsActive = 0 WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@id", id) }) > 0;
        }
    }
}