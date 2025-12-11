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

        // Persistent absolute end time in UTC. Null => no timer.
        public DateTime? EndTimeUtc { get; set; }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // Countdown in seconds used for UI binding. Updated by the window timer.
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