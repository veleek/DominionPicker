using System;
using System.IO;
#if NETFX_CORE
using Windows.UI.Xaml.Data;
using ConverterCulture = System.String;
#else
using System.Windows.Data;
using ConverterCulture = System.Globalization.CultureInfo;
#endif

namespace Ben.Data.Converters
{
    public class UriSourceConverter : IValueConverter
    {
        public UriSourceConverter()
        {
            this.Extension = ".png";
            this.BasePath = @"";
        }

        public string Extension { get; set; }

        public string BasePath { get; set; }

        public object Convert(object value, Type targetType, object parameter, ConverterCulture culture)
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

            return new Uri("ms-appx:///" + fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, ConverterCulture culture)
        {
            throw new NotImplementedException();
        }

    }
}
