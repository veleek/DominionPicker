using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ben.Data
{
    public class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String valueString = "<null>";
            if (value != null)
            {
                IFormattable formattableValue = value as IFormattable;
                if (formattableValue != null)
                {
                    valueString = formattableValue.ToString((string) parameter, null);
                }
                else
                {
                    valueString = value.ToString();
                }
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String format = parameter as String ?? "{0}";

            return String.Format(format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class ScalingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Double baseValue = System.Convert.ToDouble(value);
            Double scalar = System.Convert.ToDouble(parameter);
            Double scaledValue = baseValue * scalar;

            object result = null;
            if (targetType == typeof (Thickness))
            {
                result = new Thickness(scaledValue);
            }
            else if (targetType == typeof (CornerRadius))
            {
                result = new CornerRadius(scaledValue);
            }
            else
            {
                result = System.Convert.ChangeType(scaledValue, targetType, culture);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Double baseValue = 0;

            if (value is Thickness)
            {
                Thickness thickness = (Thickness) value;
                // Assuming a uniform thickness so just take the first value
                baseValue = thickness.Left;
            }
            else
            {
                baseValue = System.Convert.ToDouble(value);
            }

            Double scaleValue = System.Convert.ToDouble(parameter);

            return System.Convert.ChangeType(baseValue / scaleValue, targetType, culture);
        }
    }

    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                parameter = "c";
            }

            String valueString = "<null>";
            if (value != null)
            {
                IFormattable formattableValue = value as IFormattable;
                if (formattableValue != null)
                {
                    valueString = formattableValue.ToString((string) parameter, null);
                }
                else
                {
                    valueString = value.ToString();
                }
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Double.Parse(value.ToString(), NumberStyles.Currency);
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DesignerProperties.IsInDesignTool)
            {
                return Visibility.Visible;
            }

            Boolean toVisibility = targetType == typeof (Visibility);

            if (value == null)
            {
                return toVisibility ? (object) Visibility.Collapsed : (object) 0.0;
            }

            bool visibile = false;
            bool reverse = false;

            bool.TryParse(value.ToString(), out visibile);
            if (parameter != null)
            {
                bool.TryParse(parameter.ToString(), out reverse);
            }

            if (reverse)
            {
                visibile = !visibile;
            }

            if (toVisibility)
            {
                return visibile ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return visibile ? 1.0 : 0.0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof (Visibility))
            {
                Visibility visibility = Visibility.Collapsed;

                try
                {
                    visibility = (Visibility) value;
                }
                catch (InvalidCastException)
                {
                }

                return visibility == Visibility.Visible ? true : false;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0.0;
            }

            bool visibile = false;
            bool reverse = false;

            bool.TryParse(value.ToString(), out visibile);
            if (parameter != null)
            {
                bool.TryParse(parameter.ToString(), out reverse);
            }

            if (reverse)
            {
                visibile = !visibile;
            }

            return visibile ? 1.0 : 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double opacity = 0.0;

            try
            {
                opacity = (double) value;
            }
            catch (InvalidCastException)
            {
            }

            return opacity != 0.0;
        }
    }

    public class IsValidToBooleanConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean valid = false;

            // If it's a string, we check for null or emptyness.  
            String str = value as String;
            if (str != null)
            {
                valid = !String.IsNullOrWhiteSpace(str);
            }
            else
            {
                Object defaultValue = value != null && value.GetType().IsValueType
                    ? Activator.CreateInstance(value.GetType())
                    : null;
                // If the value is not equal to the default value (null or zero usually), then should be true
                valid = !Equals(value, defaultValue);
            }

            if (parameter != null && ((string) parameter).ToLower() == "true")
            {
                valid = !valid;
            }

            return valid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsValidToVisibilityConverter : IsValidToBooleanConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool valid = (bool) base.Convert(value, targetType, parameter, culture);

            return valid ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class IsTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String typeName = parameter as String;

            if (String.IsNullOrEmpty(typeName))
            {
                return false;
            }

            // If it ends with a star we'll allow it to be any type that derives from it
            if (typeName.EndsWith("*"))
            {
                typeName = typeName.TrimEnd('*');
                Type allowedType = Type.GetType(typeName);

                if (allowedType == null)
                {
                    throw new ArgumentException(@"Unable to find type " + typeName, "parameter");
                }

                return allowedType.IsInstanceOfType(value);
            }

            // If it's not specific then, the type names should match exactly
            return value.GetType().Name == (string) parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TypeToVisibilityConverter : IValueConverter
    {
        private static readonly IsTypeConverter TypeConverter = new IsTypeConverter();
        private static readonly BooleanToVisibilityConverter BoolConverter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean b = (Boolean) TypeConverter.Convert(value, targetType, parameter, culture);
            return BoolConverter.Convert(b, typeof (Visibility), null, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueOrDefaultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }
    }

    public class BrushTransparencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (Brush))
            {
                //throw new NotSupportedException("BrushTransparencyConverter is unable to convert to type " + targetType);
            }

            SolidColorBrush brush = value as SolidColorBrush;

            if (brush != null)
            {
                double alpha;
                if (!Double.TryParse(parameter as String, out alpha))
                {
                    alpha = 0.5;
                }

                Color c = brush.Color;
                c.A = (byte) (Byte.MaxValue * alpha);

                brush.Color = c;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (Brush))
            {
                throw new InvalidOperationException();
            }

            Color c = (Color) value;

            return new SolidColorBrush(c);
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <param name="value">The target data being passed to the source.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary> 
    /// Takes an enum object and returns a collection of valid values for that object
    /// </summary> 
    public class EnumValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                bool excludeDefault;
                bool.TryParse(parameter as string, out excludeDefault);

                return EnumHelper.GetValues(value.GetType(), excludeDefault);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Unable to convert enum values collection into an enum value");
        }
    }

    /// <summary>
    /// Converts a provided string value into an image brush 
    /// </summary>
    public class ImageConverter : IValueConverter
    {
        private static Dictionary<string, ImageBrush> BrushCache = new Dictionary<string, ImageBrush>();

        public ImageConverter()
        {
            this.Extension = ".png";
            this.BasePath = @".\";
        }

        public string Extension { get; set; }
        public string BasePath { get; set; }

        public static ImageBrush GetBrush(string name)
        {
            string key = name.ToLowerInvariant();

            ImageBrush brush = null;
            if (!BrushCache.TryGetValue(key, out brush))
            {
                brush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(key, UriKind.Relative)),
                    Stretch = Stretch.Uniform
                };
                BrushCache[key] = brush;
            }

            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String path = this.BasePath;
            if (parameter != null)
            {
                path = Path.Combine(path, parameter.ToString());
            }

            String fileName = value + this.Extension;
            String fullPath = Path.Combine(path, fileName);

            return GetBrush(fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Provides some debugging assistance 
    /// </summary>
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine(
                "DebugConverter::Convert - Value: {0} ({1}), TargetType: {2}, Parameter: {3}",
                value, (value ?? new object()).GetType(), targetType, parameter);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine(
                "DebugConverter::ConvertBack - Value: {0} ({1}), TargetType: {2}, Parameter: {3}",
                value, (value ?? new object()).GetType(), targetType, parameter);

            return value;
        }
    }
}