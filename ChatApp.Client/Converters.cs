using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatApp.Client
{
    /// <summary>Converts a boolean value to Visibility, where true is Visible and false is Collapsed.</summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is true ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    // <summary>Converts a UTC DateTime to local time for display.</summary>
    public class UtcToLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is DateTime dt ? DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime() : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
