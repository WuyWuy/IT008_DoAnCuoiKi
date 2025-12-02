using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyCaPhe
{
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int seconds)
            {
                if (seconds <= 0) return string.Empty;
                var ts = TimeSpan.FromSeconds(seconds);
                // ensure two-digit formatting for hours, minutes, seconds
                return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed
            throw new NotImplementedException();
        }
    }
}