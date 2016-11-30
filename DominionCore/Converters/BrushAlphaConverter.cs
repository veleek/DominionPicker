using System;

#if NETFX_CORE
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using ConverterCulture = System.String;
#else
using System.Windows.Data;
using System.Windows.Media;
using ConverterCulture = System.Globalization.CultureInfo;
#endif


namespace Ben.Data.Converters
{
    public class BrushAlphaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, ConverterCulture language)
        {
            SolidColorBrush brush = (SolidColorBrush)value;
            Color color = brush.Color;

            string alphaString = System.Convert.ToString(parameter);
            if (alphaString.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) alphaString = alphaString.Substring(2);
            color.A = Byte.Parse(alphaString, System.Globalization.NumberStyles.HexNumber);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture language)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorAlphaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, ConverterCulture language)
        {
            Color color = (Color)value;

            string alphaString = System.Convert.ToString(parameter);
            if (alphaString.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) alphaString = alphaString.Substring(2);
            color.A = Byte.Parse(alphaString, System.Globalization.NumberStyles.HexNumber);

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture language)
        {
            throw new NotImplementedException();
        }
    }
}
