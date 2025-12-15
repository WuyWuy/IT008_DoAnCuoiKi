using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using QuanLyCaPhe.DataAccess;

namespace QuanLyCaPhe
{
    public class UserStore
    {
        public UserStore() { }

        public void EnsureUsersTable()
        {
            var sql = @"
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[Users] (
                    [Id] INT IDENTITY(1,1) PRIMARY KEY,
                    [FullName] NVARCHAR(200) NOT NULL,
                    [Email] NVARCHAR(256) NOT NULL UNIQUE,
                    [Phone] NVARCHAR(50) NULL,
                    [Address] NVARCHAR(500) NULL,
                    [Gender] NVARC+-HAR(20) NULL,
                    [PasswordHash] NVARCHAR(MAX) NOT NULL,
                    [PasswordSalt] NVARCHAR(MAX) NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                );
            END";

            DBHelper.ExecuteNonQuery(sql);
        }

        public bool UserExists(string email)
        {
            var sql = "SELECT COUNT(1) FROM dbo.Users WHERE Email = @email";
            var p = new SqlParameter("@email", SqlDbType.NVarChar, 256) { Value = email };

            var res = DBHelper.ExecuteScalar(sql, new[] { p });
            return Convert.ToInt32(res) > 0;
        }

        public void RegisterUser(string fullName, string email, string phone, string address, string gender, string password)
        {
            var (hash, salt) = GeneratePasswordHash(password);

            var sql = @"INSERT INTO dbo.Users (FullName, Email, Phone, Address, Gender, PasswordHash, PasswordSalt)
                        VALUES (@fullName, @email, @phone, @address, @gender, @hash, @salt);";

            var parameters = new[]
            {
                new SqlParameter("@fullName", SqlDbType.NVarChar, 200) { Value = fullName },
                new SqlParameter("@email", SqlDbType.NVarChar, 256) { Value = email },
                new SqlParameter("@phone", SqlDbType.NVarChar, 50) { Value = (object)phone ?? DBNull.Value },
                new SqlParameter("@address", SqlDbType.NVarChar, 500) { Value = (object)address ?? DBNull.Value },
                new SqlParameter("@gender", SqlDbType.NVarChar, 20) { Value = (object)gender ?? DBNull.Value },
                new SqlParameter("@hash", SqlDbType.NVarChar, -1) { Value = hash },
                new SqlParameter("@salt", SqlDbType.NVarChar, -1) { Value = salt }
            };

            DBHelper.ExecuteNonQuery(sql, parameters);
        }

        public bool ValidateUserCredentials(string email, string password)
        {
            var sql = "SELECT PasswordHash, PasswordSalt FROM dbo.Users WHERE Email = @email";
            var p = new SqlParameter("@email", SqlDbType.NVarChar, 256) { Value = email };

            DataTable dt = DBHelper.ExecuteQuery(sql, new[] { p });

            if (dt.Rows.Count == 0) return false;

            string? hash = dt.Rows[0]["PasswordHash"].ToString();
            string? salt = dt.Rows[0]["PasswordSalt"].ToString();

            return VerifyPassword(password, hash, salt);
        }


        private static (string hashBase64, string saltBase64) GeneratePasswordHash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        private static bool VerifyPassword(string password, string storedHashBase64, string storedSaltBase64)
        {
            var salt = Convert.FromBase64String(storedSaltBase64);
            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);
            var storedHash = Convert.FromBase64String(storedHashBase64);
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}