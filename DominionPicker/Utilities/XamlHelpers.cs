using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace Ben.Dominion.Utilities
{
    public static class XamlHelpers
    {
        /// <summary>
        /// Generate a visually appealing XAML object that can be rendered
        /// as part of the UI using a raw text stream as input.  
        /// </summary>
        /// <param name="stream">The stream containing the text content to be converted</param>
        /// <param name="resources">A dictionary containing any custom resources used to generate the output</param>
        /// <returns>A FrameworkElement that can be added to the UI</returns>
        public static FrameworkElement GenerateXamlFromText(Stream stream)
        {
            StackPanel container = new StackPanel();

            using (StreamReader reader = new StreamReader(stream))
            {
                String line;
                bool lastWasEmpty = true;
                do
                {
                    line = reader.ReadLine();
                    if (String.IsNullOrEmpty(line))
                    {
                        Rectangle r = new Rectangle
                        {
                            Height = 20,
                        };
                        container.Children.Add(r);
                        lastWasEmpty = true;
                    }
                    else
                    {
                        FrameworkElement fe;
                        if (lastWasEmpty)
                        {
                            fe = new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Text = line,
                                Style = (Style)Application.Current.Resources["PhoneTextNormalStyle"],
                            };
                        }
                        else
                        {
                            fe = new ContentPresenter
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                ContentTemplate = (DataTemplate)Application.Current.Resources["BulletedItem"],
                                Content = line,
                            };
                        }

                        lastWasEmpty = false;
                        container.Children.Add(fe);
                    }
                } while (line != null);
            }

            return container;
        }
    }

    public class XamlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string streamName = value.ToString();

            StreamResourceInfo sri = Application.GetResourceStream(new Uri("./Resources/Changes.txt", UriKind.Relative));
            if (sri == null)
            {
                return null;
            }

            return XamlHelpers.GenerateXamlFromText(sri.Stream);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Unable to convert XAML back into a text document");
        }
    }
}
