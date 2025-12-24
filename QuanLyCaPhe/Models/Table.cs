using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;

namespace QuanLyCaPhe.Models
{
    public class Table : INotifyPropertyChanged
    {
        private string _status = "Free";
        private int _countdown;
        private bool _isActive = true; // Mặc định là bật
        private string _note = "";

        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? EndTimeUtc { get; set; }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // --- [MỚI] Trạng thái Hoạt động (Bật/Tắt) ---
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        // --- [MỚI] Ghi chú ---
        public string Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        public int Countdown
        {
            get => _countdown;
            set { _countdown = value; OnPropertyChanged(); }
        }

        public Table() { }

        // Constructor nhận DataRow từ DAO
        public Table(DataRow row)
        {
            Id = (int)row["Id"];
            Name = row["TableName"].ToString();
            Status = row["Status"].ToString();

            // Xử lý IsActive (nếu null thì coi như true)
            if (row["IsActive"] != DBNull.Value)
                IsActive = (bool)row["IsActive"];

            // Xử lý Note
            if (row["Note"] != DBNull.Value)
                Note = row["Note"].ToString();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }
    }
}