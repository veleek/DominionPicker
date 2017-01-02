using System;
using System.IO;
using System.Threading.Tasks;
using Ben.Utilities;

using Windows.ApplicationModel;
#if NETFX_CORE
using Windows.UI.Xaml.Data;
using ConverterCulture = System.String;
#else
using System.Windows.Data;
using ConverterCulture = System.Globalization.CultureInfo;
#endif

namespace Ben.Data.Converters
{

    public class XamlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            try
            {
                string streamName = value.ToString();

                Stream xamlStream = null;
                Task.Run(async () =>
                {
                    xamlStream = await FileUtility.OpenApplicationStreamAsync(streamName);
                }).Wait();
                
                return XamlHelpers.GenerateXamlFromText(xamlStream);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            throw new InvalidOperationException("Unable to convert XAML back into a text document");
        }
    }
}
