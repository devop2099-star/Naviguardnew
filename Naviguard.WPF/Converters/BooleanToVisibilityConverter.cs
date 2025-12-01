using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Naviguard.WPF.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;
            if (value is bool boolValue)
            {
                isVisible = boolValue;
            }

            if (Invert)
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
