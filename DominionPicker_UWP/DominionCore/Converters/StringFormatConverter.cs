using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace Ben.Data.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            string format = parameter as string ?? "{0}";
            Debug.WriteLine("Using Format " + format);
            return string.Format(format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new InvalidOperationException();
        }
    }
}
