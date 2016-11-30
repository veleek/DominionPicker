using System;
using System.Diagnostics;
#if NETFX_CORE
using Windows.UI.Xaml.Data;
using ConverterCulture = System.String;
#else
using System.Windows.Data;
using ConverterCulture = System.Globalization.CultureInfo;
#endif

namespace Ben.Data.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            string format = parameter as string ?? "{0}";
            Debug.WriteLine("Using Format " + format);
            return string.Format(format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            throw new InvalidOperationException();
        }
    }
}
