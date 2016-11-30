using System;
using System.Reflection;
#if NETFX_CORE
using Windows.UI.Xaml.Data;
using ConverterCulture = System.String;
#else
using System.Windows.Data;
using ConverterCulture = System.Globalization.CultureInfo;
#endif

namespace Ben.Data.Converters
{
    public class LocalizeConverter
       : IValueConverter
    {
        public Localizer Localizer { get; protected set; }


        public LocalizeConverter(Localizer localizer)
        {
            this.Localizer = localizer;
        }

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            if (!targetType.GetTypeInfo().IsAssignableFrom(typeof(String).GetTypeInfo()))
            {
                throw new ArgumentException(string.Format("Unable to convert to type {0}", targetType));
            }
            return Localizer.GetLocalizedValue(value, parameter as string, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            throw new InvalidOperationException("Unable to convert localized value back into object");
        }

    }
}