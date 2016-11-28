using System;
using System.IO;
using Windows.UI.Xaml.Data;

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

            return new Uri("ms-appx:///" + fullPath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

    }
}
