using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using QuanLyCaPhe.Views.Admin;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace QuanLyCaPhe.DAO
{
    public class TopProductDAO
    {
        private static TopProductDAO instance;
        public static TopProductDAO Instance
        {
            get { if (instance == null) instance = new TopProductDAO(); return instance; }
            private set { instance = value; }
        }
        private TopProductDAO() { }

        public List<TopProduct> GetTop3BestSellers()
        {
            List<TopProduct> list = new List<TopProduct>();

            // Query: Join bảng BillInfo, Bill và Product
            // Điều kiện: Hóa đơn đã thanh toán (Status=1) VÀ Trong tháng/năm hiện tại
            string query = @"
                SELECT TOP 3 p.ProName, SUM(bi.Count) as TotalCount
                FROM BillInfos bi
                JOIN Bills b ON bi.BillId = b.Id
                JOIN Products p ON bi.ProId = p.Id
                WHERE b.Status = 1 
                  AND MONTH(b.DateCheckIn) = MONTH(GETDATE()) 
                  AND YEAR(b.DateCheckIn) = YEAR(GETDATE())
                GROUP BY p.ProName
                ORDER BY TotalCount DESC";

            try
            {
                DataTable data = DBHelper.ExecuteQuery(query);

                int rank = 1;
                foreach (DataRow item in data.Rows)
                {
                    list.Add(new TopProduct
                    {
                        Name = item["ProName"].ToString(),
                        Count = (int)item["TotalCount"],
                        Rank = rank++
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mày đây r hommie!!: " + ex.Message);
            }

            return list;
        }
    }
}