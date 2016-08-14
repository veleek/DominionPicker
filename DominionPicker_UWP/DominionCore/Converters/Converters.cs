using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Ben.Data
{
    public class StringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            String valueString = "<null>";
            if (value != null)
            {
                IFormattable formattableValue = value as IFormattable;
                if (formattableValue != null)
                {
                    valueString = formattableValue.ToString((string)parameter, null);
                }
                else
                {
                    valueString = value.ToString();
                }
            }
            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new InvalidOperationException();
        }

    }

    public class ScalingConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Double baseValue = System.Convert.ToDouble(value);
            Double scalar = System.Convert.ToDouble(parameter);
            Double scaledValue = baseValue * scalar;
            object result = null;
            if (targetType == typeof(Windows.UI.Xaml.Thickness))
            {
                result = new Windows.UI.Xaml.Thickness(scaledValue);
            }
            else if (targetType == typeof(CornerRadius))
            {
                result = new CornerRadius(scaledValue);
            }
            else
            {
                result = System.Convert.ChangeType(scaledValue, targetType);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            Double baseValue = 0;
            if (value is Windows.UI.Xaml.Thickness)
            {
                Windows.UI.Xaml.Thickness thickness = (Windows.UI.Xaml.Thickness)value;
                // Assuming a uniform thickness so just take the first value
                baseValue = thickness.Left;
            }
            else
            {
                baseValue = System.Convert.ToDouble(value);
            }
            Double scaleValue = System.Convert.ToDouble(parameter);
            return System.Convert.ChangeType(baseValue / scaleValue, targetType);
        }

    }

    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
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
                    valueString = formattableValue.ToString((string)parameter, null);
                }
                else
                {
                    valueString = value.ToString();
                }
            }
            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return Double.Parse(value.ToString(), NumberStyles.Currency);
        }

    }

    public class BooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return Visibility.Visible;
            }
            Boolean toVisibility = targetType == typeof(Visibility);
            if (value == null)
            {
                return toVisibility ? (object)Visibility.Collapsed : (object)0.0;
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

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            if (targetType == typeof(Visibility))
            {
                Visibility visibility = Visibility.Collapsed;
                try
                {
                    visibility = (Visibility)value;
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

        public object Convert(object value, Type targetType, object parameter, string culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            double opacity = 0.0;
            try
            {
                opacity = (double)value;
            }
            catch (InvalidCastException)
            {
            }
            return opacity != 0.0;
        }

    }

    public class IsValidToBooleanConverter : IValueConverter
    {

        public virtual object Convert(object value, Type targetType, object parameter, string culture)
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
                Object defaultValue = value != null && value.GetType().GetTypeInfo().IsValueType ? Activator.CreateInstance(value.GetType()) : null;
                // If the value is not equal to the default value (null or zero usually), then should be true
                valid = !Equals(value, defaultValue);
            }
            if (parameter != null && ((string)parameter).ToLower() == "true")
            {
                valid = !valid;
            }
            return valid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

    }

    public class IsValidToVisibilityConverter : IsValidToBooleanConverter
    {

        public override object Convert(object value, Type targetType, object parameter, String culture)
        {
            bool valid = (bool)base.Convert(value, targetType, parameter, culture);
            return valid ? Visibility.Visible : Visibility.Collapsed;
        }

    }

    public class IsTypeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string culture)
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
            return value.GetType().Name == (string)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

    }

    public class TypeToVisibilityConverter
       : IValueConverter
    {
        private static readonly IsTypeConverter TypeConverter = new IsTypeConverter();
        private static readonly BooleanToVisibilityConverter BoolConverter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            Boolean b = (Boolean)TypeConverter.Convert(value, targetType, parameter, culture);
            return BoolConverter.Convert(b, typeof(Visibility), null, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

    }

    public class ValueOrDefaultConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return value ?? parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return value ?? parameter;
        }

    }

    public class BrushTransparencyConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (targetType != typeof(Windows.UI.Xaml.Media.Brush))
            {
                //throw new NotSupportedException("BrushTransparencyConverter is unable to convert to type " + targetType);
            }
            Windows.UI.Xaml.Media.SolidColorBrush brush = value as Windows.UI.Xaml.Media.SolidColorBrush;
            if (brush != null)
            {
                double alpha;
                if (!Double.TryParse(parameter as String, out alpha))
                {
                    alpha = 0.5;
                }
                Windows.UI.Color c = brush.Color;
                c.A = (byte)(Byte.MaxValue * alpha);
                brush.Color = c;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
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
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException();
            }
            Windows.UI.Color c = (Windows.UI.Color)value;
            return new SolidColorBrush(c);
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return null;
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
            this.BasePath = @"";
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
                    ImageSource = new BitmapImage(new Uri("ms-appx:///" + key)),
                    Stretch = Stretch.Uniform
                };
                BrushCache[key] = brush;
            }
            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            String path = this.BasePath;
            if (parameter != null)
            {
                path = Path.Combine(path, parameter.ToString());
            }
            String fileName = value.ToString();
            if (!Path.HasExtension(fileName))
            {
                fileName += this.Extension;
            }
            String fullPath = Path.Combine(path, fileName);
            return GetBrush(fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

    }
}