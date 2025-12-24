using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyCaPhe.DAO
{
    public class PaymentAccountDAO
    {
        private static PaymentAccountDAO instance;
        public static PaymentAccountDAO Instance
        {
            get { if (instance == null) instance = new PaymentAccountDAO(); return instance; }
            private set { instance = value; }
        }
        private PaymentAccountDAO() { }

        public List<PaymentAccount> GetList()
        {
            List<PaymentAccount> list = new List<PaymentAccount>();
            DataTable data = DBHelper.ExecuteQuery("SELECT * FROM PaymentAccounts");
            foreach (DataRow item in data.Rows) list.Add(new PaymentAccount(item));
            return list;
        }

        // Lấy tài khoản đang kích hoạt để StaffWindow dùng
        public PaymentAccount GetActiveAccount()
        {
            DataTable data = DBHelper.ExecuteQuery("SELECT TOP 1 * FROM PaymentAccounts WHERE IsActive = 1");
            if (data.Rows.Count > 0) return new PaymentAccount(data.Rows[0]);
            return null;
        }

        public bool Insert(string bankName, string bin, string accNo, string accName, string template)
        {
            string query = "INSERT INTO PaymentAccounts (BankName, BankBin, AccountNumber, AccountName, Template, IsActive) VALUES (@name, @bin, @no, @accName, @temp, 0)";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@name", bankName),
                new SqlParameter("@bin", bin),
                new SqlParameter("@no", accNo),
                new SqlParameter("@accName", accName),
                new SqlParameter("@temp", template)
            }) > 0;
        }

        // Hàm này quan trọng: Set 1 cái Active, các cái còn lại thành False
        public bool SetActive(int id)
        {
            string queryReset = "UPDATE PaymentAccounts SET IsActive = 0";
            DBHelper.ExecuteNonQuery(queryReset);

            string querySet = "UPDATE PaymentAccounts SET IsActive = 1 WHERE Id = " + id;
            return DBHelper.ExecuteNonQuery(querySet) > 0;
        }

        public bool Delete(int id)
        {
            return DBHelper.ExecuteNonQuery("DELETE PaymentAccounts WHERE Id = " + id) > 0;
        }

        // Cập nhật thông tin tài khoản (không cập nhật IsActive)
        public bool Update(int id, string bankName, string bin, string accNo, string accName, string template)
        {
            string query = "UPDATE PaymentAccounts SET BankName = @name, BankBin = @bin, AccountNumber = @no, AccountName = @accName, Template = @temp WHERE Id = @id";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                    new SqlParameter("@name", bankName),
                    new SqlParameter("@bin", bin),
                    new SqlParameter("@no", accNo),
                    new SqlParameter("@accName", accName),
                    new SqlParameter("@temp", template),
                    new SqlParameter("@id", id)
                    }) > 0;
        }
    }
}