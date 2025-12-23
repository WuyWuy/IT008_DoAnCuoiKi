using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace QuanLyCaPhe.DAO
{
    public partial class BillDAO
    {
        private static BillDAO instance;
        public static BillDAO Instance
        {
            get { if (instance == null) instance = new BillDAO(); return instance; }
            private set { instance = value; }
        }
        private BillDAO() { }

        // --- LẤY DANH SÁCH HÓA ĐƠN (Cho BillsPage) ---
        public List<Bill> GetListBills()
        {
            List<Bill> list = new List<Bill>();
            // include new columns PaymentMethod and TimeUsedHours
            string query = @"
                SELECT b.*, t.TableName, u.FullName AS StaffName, b.PaymentMethod, b.TimeUsedHours
                FROM Bills b
                LEFT JOIN TableCoffees t ON b.TableId = t.Id
                LEFT JOIN Users u ON b.UserId = u.Id
                WHERE b.Status = 1 -- Chỉ lấy hóa đơn đã thanh toán
                ORDER BY b.DateCheckIn DESC";

            DataTable data = DBHelper.ExecuteQuery(query);
            foreach (DataRow item in data.Rows)
            {
                Bill bill = new Bill(item);
                list.Add(bill);
            }
            return list;
        }

        // --- TÌM KIẾM HÓA ĐƠN ---
        public List<Bill> SearchBill(string keyword)
        {
            List<Bill> list = new List<Bill>();
            string query = @"
                SELECT b.*, t.TableName, u.FullName AS StaffName, b.PaymentMethod, b.TimeUsedHours
                FROM Bills b
                LEFT JOIN TableCoffees t ON b.TableId = t.Id
                LEFT JOIN Users u ON b.UserId = u.Id
                WHERE b.Status = 1 
                AND (t.TableName LIKE @key OR u.FullName LIKE @key)";

            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] {
                new SqlParameter("@key", "%" + keyword + "%")
            });

            foreach (DataRow item in data.Rows)
            {
                list.Add(new Bill(item));
            } 
                
            return list;
        }

        public DataTable GetBillDetails(int billId)
        {
            string query = @"
                SELECT 
                    p.ProName AS [TenMon], 
                    bi.Count AS [SoLuong], 
                    p.Price AS [DonGia], 
                    (p.Price * bi.Count) AS [ThanhTien]
                FROM BillInfos bi
                JOIN Products p ON bi.ProId = p.Id
                WHERE bi.BillId = @billId";

            return DBHelper.ExecuteQuery(query, new SqlParameter[] {
                new SqlParameter("@billId", billId)
            });
        }

        // --- Thêm hóa đơn mới and return id ---
        public int InsertBill(DateTime dateCheckIn, DateTime? dateCheckOut, int status, int discount, decimal totalPrice, int tableId, int? userId, string paymentMethod = null, int? timeUsedHours = null)
        {
            string query = @"
INSERT INTO Bills (DateCheckIn, DateCheckOut, Status, Discount, TotalPrice, TableId, UserId, PaymentMethod, TimeUsedHours)
VALUES (@ci, @co, @st, @disc, @total, @tableId, @userId, @paymentMethod, @timeUsedHours);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            object result = DBHelper.ExecuteScalar(query, new SqlParameter[] {
                new SqlParameter("@ci", dateCheckIn),
                new SqlParameter("@co", dateCheckOut ?? (object)DBNull.Value),
                new SqlParameter("@st", status),
                new SqlParameter("@disc", discount),
                new SqlParameter("@total", totalPrice),
                new SqlParameter("@tableId", tableId),
                new SqlParameter("@userId", userId ?? (object)DBNull.Value),
                new SqlParameter("@paymentMethod", paymentMethod ?? (object)DBNull.Value),
                new SqlParameter("@timeUsedHours", timeUsedHours ?? (object)DBNull.Value)
            });

            if (result == null) return -1;
            return Convert.ToInt32(result);
        }
    }
}