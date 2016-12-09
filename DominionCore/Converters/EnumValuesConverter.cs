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
    /// Takes an enum object and returns a collection of valid values for that object
    /// </summary> 
    public class EnumValuesConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            if (value == null)
            {
                return null;
            }
            bool excludeDefault;
            bool.TryParse(parameter as string, out excludeDefault);
            // If the provided object is a type, then just use that, otherwise
            // we'll use the type of the value.
            Type valueType = value as Type ?? value.GetType();
            return EnumHelper.GetValues(valueType, excludeDefault);
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            throw new InvalidOperationException("Unable to convert enum values collection into an enum value");
        }

    }
}