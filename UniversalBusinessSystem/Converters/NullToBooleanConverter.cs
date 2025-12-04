using System;
using System.Globalization;
using System.Windows.Data;

namespace UniversalBusinessSystem.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value != null;
            if (parameter is string invertLiteral && bool.TryParse(invertLiteral, out var invert) && invert)
            {
                return !result;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
