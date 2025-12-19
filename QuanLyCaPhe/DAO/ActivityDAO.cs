using QuanLyCaPhe.DataAccess; // Sử dụng DBHelper của bạn
using QuanLyCaPhe.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyCaPhe.DAO
{
    public class ActivityDAO
    {
        private static ActivityDAO instance;
        public static ActivityDAO Instance
        {
            get { if (instance == null) instance = new ActivityDAO(); return instance; }
            private set { instance = value; }
        }

        private ActivityDAO() { }

        // ... (các using giữ nguyên)

        public bool InsertActivity(string type, string desc, string detail)
        {
            // Truyền thời gian từ Code xuống để đồng bộ với giao diện
            string query = "INSERT INTO Activities (ActivityType, Description, Detail, TimeAgo) VALUES ( @type , @desc , @detail , @time )";

            int result = DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
        new SqlParameter("@type", type),
        new SqlParameter("@desc", desc),
        new SqlParameter("@detail", detail),
        new SqlParameter("@time", DateTime.Now) // <--- QUAN TRỌNG: Dùng giờ máy tính
    });
            return result > 0;
        }

        public List<Activity> GetRecentActivities(int limit = 20)
        {
            List<Activity> list = new List<Activity>();
            string query = $"SELECT TOP {limit} * FROM Activities ORDER BY TimeAgo DESC";
            DataTable data = DBHelper.ExecuteQuery(query);

            foreach (DataRow row in data.Rows)
            {
                Activity act = new Activity
                {
                    Id = (int)row["Id"],
                    ActivityType = row["ActivityType"].ToString(),
                    Description = row["Description"].ToString(),
                    Detail = row["Detail"].ToString(),
                    CreatedDate = (DateTime)row["TimeAgo"] 
                };
                SetupDisplayProperties(act);
                list.Add(act);
            }
            return list;
        }

        // Cấu hình Icon cho đẹp
        private void SetupDisplayProperties(Activity act)
        {
            switch (act.ActivityType)
            {
                case "Payment": // Icon Check Xanh
                    act.IconPath = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 17l-5-5 1.41-1.41L10 16.17l7.59-7.59L19 10z";
                    act.IconColor = "#10B981";
                    break;
                case "Order": // Icon Cộng Xanh Dương
                    act.IconPath = "M20 2H4v20h16V2zm-4 12h-4v4h-2v-4H8v-2h4V8h2v4h2v2z";
                    act.IconColor = "#3B82F6";
                    break;
                default: // Icon Chuông/Thông báo
                    act.IconPath = "M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z";
                    act.IconColor = "#64748B";
                    break;
            }
        }
    }
}