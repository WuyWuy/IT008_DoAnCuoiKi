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
            // JOIN để lấy tên bàn và tên nhân viên thay vì chỉ lấy ID
            string query = @"
                SELECT b.*, t.TableName, u.FullName AS StaffName
                FROM Bills b
                LEFT JOIN TableCoffees t ON b.TableId = t.Id
                LEFT JOIN Users u ON b.UserId = u.Id
                WHERE b.Status = 1 -- Chỉ lấy hóa đơn đã thanh toán
                ORDER BY b.DateCheckIn DESC"; // Mới nhất lên đầu

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
                SELECT b.*, t.TableName, u.FullName AS StaffName
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

        // --- LẤY CHI TIẾT MÓN (Giữ nguyên của bạn) ---
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

        // --- Thêm hóa đơn mới và trả về Id mới tạo ---
        public int InsertBill(DateTime dateCheckIn, DateTime? dateCheckOut, int status, int discount, decimal totalPrice, int tableId, int? userId)
        {
            string query = @"INSERT INTO Bills (DateCheckIn, DateCheckOut, Status, Discount, TotalPrice, TableId, UserId)
VALUES (@ci, @co, @st, @disc, @total, @tableId, @userId); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            object result = DBHelper.ExecuteScalar(query, new SqlParameter[] {
                new SqlParameter("@ci", dateCheckIn),
                new SqlParameter("@co", dateCheckOut ?? (object)DBNull.Value),
                new SqlParameter("@st", status),
                new SqlParameter("@disc", discount),
                new SqlParameter("@total", totalPrice),
                new SqlParameter("@tableId", tableId),
                new SqlParameter("@userId", userId ?? (object)DBNull.Value)
            });

            if (result == null) return -1;
            return Convert.ToInt32(result);
        }
    }
}