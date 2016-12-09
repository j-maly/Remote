using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Remote.Utils
{
    [ValueConversion(typeof(IEnumerable<string>), typeof(string))]
    public class LinesToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a string");

            if (value == null)
                return null;
            if (value is IEnumerable)
                return string.Join(Environment.NewLine, ((IEnumerable)value).Cast<string>());
            else
                throw new InvalidOperationException("Expecting an enumeration of strings");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}