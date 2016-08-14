using System;
using Windows.UI.Xaml.Data;

namespace Ben.Data
{
    /// <summary>
    /// Provides some debugging assistance 
    /// </summary>
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            System.Diagnostics.Debug.WriteLine("DebugConverter::Convert - Value: {0} ({1}), TargetType: {2}, Parameter: {3}", value, (value ?? new object()).GetType(), targetType, parameter);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            System.Diagnostics.Debug.WriteLine("DebugConverter::ConvertBack - Value: {0} ({1}), TargetType: {2}, Parameter: {3}", value, (value ?? new object()).GetType(), targetType, parameter);
            return value;
        }
    }
}
