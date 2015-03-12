using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Windows.Shapes;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System;
using Ben.Utilities;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Path = System.IO.Path;

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

        private async void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Button b = sender as Button;
                
                IStorageFile rulesFile = await this.GetRulesFile(b.GetTag<string>());
                if (await Launcher.LaunchFileAsync(rulesFile))
                {
                    AppLog.Instance.Log("Opened Rules PDF " + rulesFile.Name);
                }
            }
            catch (Exception ex)
            {
                AppLog.Instance.Error("Failed to load Rules PDF", ex);
            }
        }

        private async Task<IStorageFile> GetRulesFile(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            String fileName = Path.GetFileName(url);

            if (fileName == null)
            {
                throw new ArgumentException("Invalid URL provided for rules PDF.");
            }

            var folder = ApplicationData.Current.LocalFolder;
            IStorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            var properties = await file.GetBasicPropertiesAsync();

            if (properties.Size == 0)
            {
                AppLog.Instance.Debug("Unable to find rules PDF for {0}. Downloading from {1}", fileName, url);
                // The file wasn't found so let's download the file.
                using (HttpClient client = new HttpClient())
                {
                    Stream rulesFileStream = await client.GetStreamAsync(url);

                    using (var storageFileStream = await file.OpenStreamForWriteAsync())
                    {
                        await rulesFileStream.CopyToAsync(storageFileStream);

                        AppLog.Instance.Debug("Finished downloading rules file.");
                    }
                }
            }
            else
            {
                AppLog.Instance.Debug("Rules PDF {0} already exists locally.");
                //var f = new BackgroundTransferRequest(new Uri(url), );
            }

            return file;
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