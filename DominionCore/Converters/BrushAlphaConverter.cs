using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ben.Data.Converters
{
    public class BrushAlphaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush brush = (SolidColorBrush)value;
            Color color = brush.Color;

            string alphaString = System.Convert.ToString(parameter);
            if (alphaString.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) alphaString = alphaString.Substring(2);
            color.A = Byte.Parse(alphaString, System.Globalization.NumberStyles.HexNumber);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorAlphaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color color = (Color)value;

            string alphaString = System.Convert.ToString(parameter);
            if (alphaString.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) alphaString = alphaString.Substring(2);
            color.A = Byte.Parse(alphaString, System.Globalization.NumberStyles.HexNumber);

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
