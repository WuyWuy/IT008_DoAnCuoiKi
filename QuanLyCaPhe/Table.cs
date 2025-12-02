using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyCaPhe
{
    public class Table : INotifyPropertyChanged
    {
        private string _status = "Free"; // Free, Selected, Busy
        private int _countdown;

        public int Id { get; set; }
        public string? Name { get; set; }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public int Countdown
        {
            get => _countdown;
            set { _countdown = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }
    }
}

