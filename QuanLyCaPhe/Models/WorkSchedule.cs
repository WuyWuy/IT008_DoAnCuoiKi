using System;
using System.Data;

namespace QuanLyCaPhe.Models
{
    public class WorkSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan StartTime { get; set; } // SQL Time -> C# TimeSpan
        public TimeSpan EndTime { get; set; }
        public string Notes { get; set; }

        public WorkSchedule() { }

        public WorkSchedule(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.UserId = (int)row["UserId"];
            this.WorkDate = (DateTime)row["WorkDate"];

            // Ép kiểu Time
            this.StartTime = (TimeSpan)row["StartTime"];
            this.EndTime = (TimeSpan)row["EndTime"];

            // Kiểm tra Null cho Notes
            this.Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : "";
        }
    }
}