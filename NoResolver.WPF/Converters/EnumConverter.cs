using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace NoResolver.WPF.Converters
{

    /// <summary>
    /// Converts an enum to allow binding
    /// https://stackoverflow.com/a/2908885/7466296
    /// </summary>
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}
