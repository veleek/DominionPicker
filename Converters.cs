using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace Ben.Dominion
{
    public class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value ?? "<null>").ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean visible = (Boolean)value;

            if (parameter != null)
            {
                String swap = (String)parameter;
                if (swap.ToLower() == "true")
                {
                    visible = !visible;
                }
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visible = (Visibility)value;
            return visible == Visibility.Visible ? true : false;
        }
    }

    public class IsValidToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean isVisible = false;

            // If the value is null, then we just return false
            if (value != null)
            {
                // If the value is not null, then it's possibly an enum
                if (value is Int32)
                {
                    Int32 enumValue = (Int32)value;

                    // So if it's 0 (ideally, the none value), then we return false
                    if (enumValue != 0)
                    {
                        isVisible = true;
                    }
                }
                else
                {
                    isVisible = true;
                }
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
                return allowedType.IsInstanceOfType(value);
            }
            else
            {
                // If it's not specific then, the type names should match exactly
                return value.GetType().Name == (string)parameter;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TypeToVisibilityConverter : IValueConverter
    {
        IsTypeConverter typeConverter = new IsTypeConverter();
        BooleanToVisibilityConverter boolConverter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean b = (Boolean)typeConverter.Convert(value, targetType, parameter, culture);
            return boolConverter.Convert(b, typeof(Visibility), null, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueOrDefaultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value ?? parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value ?? parameter;
        }
    }
}
