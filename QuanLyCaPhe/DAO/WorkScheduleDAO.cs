using QuanLyCaPhe.DataAccess;
using QuanLyCaPhe.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyCaPhe.DAO
{
    public class WorkScheduleDAO
    {
        private static WorkScheduleDAO instance;
        public static WorkScheduleDAO Instance
        {
            get { if (instance == null) instance = new WorkScheduleDAO(); return instance; }
            private set { instance = value; }
        }
        private WorkScheduleDAO() { }

        public List<WorkSchedule> GetListByDate(DateTime date)
        {
            List<WorkSchedule> list = new List<WorkSchedule>();
            string query = "SELECT * FROM WorkSchedules WHERE CAST(WorkDate AS DATE) = CAST(@date AS DATE)";
            DataTable data = DBHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@date", date) });
            foreach (DataRow item in data.Rows) list.Add(new WorkSchedule(item));
            return list;
        }

        public bool RegisterSchedule(int userId, DateTime date, TimeSpan start, TimeSpan end, string note)
        {
            string query = "INSERT INTO WorkSchedules (UserId, WorkDate, StartTime, EndTime, Notes) VALUES (@uid, @date, @start, @end, @note)";
            return DBHelper.ExecuteNonQuery(query, new SqlParameter[] {
                new SqlParameter("@uid", userId),
                new SqlParameter("@date", date),
                new SqlParameter("@start", start),
                new SqlParameter("@end", end),
                new SqlParameter("@note", note)
            }) > 0;
        }

        public bool DeleteSchedule(int id)
        {
            return DBHelper.ExecuteNonQuery("DELETE WorkSchedules WHERE Id = " + id) > 0;
        }
    }
}