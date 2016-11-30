using System;
#if NETFX_CORE
using Windows.UI.Xaml.Data;
using ConverterCulture = System.String;
#else
using System.Windows.Data;
using ConverterCulture = System.Globalization.CultureInfo;
#endif

namespace Ben.Data.Converters
{
    public class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, ConverterCulture language)
        {
            return value?.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture language)
        {
            throw new InvalidOperationException();
        }
    }
}
