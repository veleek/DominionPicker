using System;
using System.IO;
using System.Threading.Tasks;
using Ben.Utilities;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Data;

namespace Ben.Data.Converters
{

    public class XamlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            try
            {
                string streamName = value.ToString();

                Stream xamlStream = null;
                Task.Run(async () =>
                {
                    Task<Stream> xamlStreamTask = Package.Current.InstalledLocation.SafeOpenStreamForReadAsync(streamName);
                    xamlStream = await xamlStreamTask;
                }).Wait();
                
                return XamlHelpers.GenerateXamlFromText(xamlStream);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new InvalidOperationException("Unable to convert XAML back into a text document");
        }
    }
}
