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
    /// <summary>
    /// Provides some debugging assistance 
    /// </summary>
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            System.Diagnostics.Debug.WriteLine("DebugConverter::Convert - Value: {0} ({1}), TargetType: {2}, Parameter: {3}", value, (value ?? new object()).GetType(), targetType, parameter);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            System.Diagnostics.Debug.WriteLine("DebugConverter::ConvertBack - Value: {0} ({1}), TargetType: {2}, Parameter: {3}", value, (value ?? new object()).GetType(), targetType, parameter);
            return value;
        }
    }
}
