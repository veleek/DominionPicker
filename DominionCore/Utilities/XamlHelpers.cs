using System.IO;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Text;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
#endif

namespace Ben.Utilities
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
            Style bodyStyle = (Style)Application.Current.Resources["BodyTextBlockStyle"];
            DataTemplate bulletedItemTemplate = (DataTemplate)Application.Current.Resources["BulletedItem"];

            StackPanel container = new StackPanel();
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                bool lastWasEmpty = true;
                do
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
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
                            if (line.StartsWith("## ")) line = line.Substring(3);
                            fe = new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Text = line,
                                Style = bodyStyle,
                                FontWeight = FontWeights.Bold,
                            };
                        }
                        else
                        {
                            if(line.StartsWith("* ")) line = line.Substring(2);
                            fe = new ContentPresenter
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                ContentTemplate = bulletedItemTemplate,
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
}