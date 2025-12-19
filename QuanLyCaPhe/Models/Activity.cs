using System;

namespace QuanLyCaPhe.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }

        // 1. Đổi tên cho đúng bản chất: Đây là mốc thời gian
        public DateTime CreatedDate { get; set; }

        public string IconPath { get; set; }
        public string IconColor { get; set; }

        // 2. Đây mới là chuỗi hiển thị "Cách đây..."
        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - CreatedDate;
                if (span.TotalMinutes < 1) return "Vừa xong";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
                if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
                return CreatedDate.ToString("dd/MM HH:mm");
            }
        }
    }
}