using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Ben.Dominion
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            this.InitializeComponent();

            this.VersionTextBlock.Text = this.GetType().Assembly.ToString().Split('=', ',')[2];

            StreamResourceInfo sri = Application.GetResourceStream(new Uri("./Resources/Changes.txt", UriKind.Relative));
            if (sri != null)
            {
                StackPanel changes = new StackPanel();

                using (StreamReader reader = new StreamReader(sri.Stream))
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
                            changes.Children.Add(r);
                            lastWasEmpty = true;
                        }
                        else
                        {
                            FrameworkElement fe = null;
                            if (lastWasEmpty)
                            {
                                fe = new TextBlock
                                {
                                    TextWrapping = TextWrapping.Wrap,
                                    Text = line,
                                    Style = (Style) Application.Current.Resources["PhoneTextNormalStyle"],
                                };
                            }
                            else
                            {
                                fe = new ContentPresenter
                                {
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    ContentTemplate = (DataTemplate) this.LayoutRoot.Resources["BulletedItem"],
                                    Content = line,
                                };
                            }

                            lastWasEmpty = false;
                            changes.Children.Add(fe);
                        }
                    } while (line != null);
                }

                this.ChangesScrollViewer.Content = changes;
            }
        }

        private void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "randall.ben@gmail.com";
            emailComposeTask.Subject = "Dominion Picker";
            emailComposeTask.Body = "Comments:\nRequests:\n";
            emailComposeTask.Show();
        }

        private void RateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
                reviewTask.Show();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MarketplaceDetailTask detailTask = new MarketplaceDetailTask();
                detailTask.ContentType = MarketplaceContentType.Applications;
                detailTask.Show();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            String url = b.Tag as String;

            WebBrowserTask browserTask = new WebBrowserTask();
            browserTask.Uri = new Uri(url);
            browserTask.Show();
        }

        private void MarketplaceButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            String tag = b.Tag as String;

            MarketplaceDetailTask dt = new MarketplaceDetailTask();
            dt.ContentIdentifier = tag;
            dt.ContentType = MarketplaceContentType.Applications;

            dt.Show();
        }
    }
}