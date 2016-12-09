using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Storage;
using Windows.System;
using Ben.Dominion.Utilities;
using Ben.Utilities;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using Ben.Data;

namespace Ben.Dominion
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            this.InitializeComponent();

            string version = this.GetType().Assembly.ToString().Split('=', ',')[2];
#if DEBUG
            version += " - DEBUG";
#endif
            this.VersionTextBlock.Text = version;


            this.RulesInfo.ItemsSource = new[]
            {
                new RulesInfoItem(CardSet.Base),
                new RulesInfoItem(CardSet.Intrigue),
                new RulesInfoItem(CardSet.Seaside),
                new RulesInfoItem(CardSet.Alchemy),
                new RulesInfoItem(CardSet.Prosperity),
                new RulesInfoItem(CardSet.Cornucopia),
                new RulesInfoItem(CardSet.Hinterlands),
                new RulesInfoItem(CardSet.DarkAges),
                new RulesInfoItem(CardSet.Guilds),
                new RulesInfoItem(CardSet.Adventures),
            };
        }

        public class RulesInfoItem
        {

            public RulesInfoItem(CardSet set)
            {
                string rulesUrl;
                switch (set)
                {
                    case CardSet.Base:
                        rulesUrl = "http://riograndegames.com/getFile.php?id=348";
                        break;
                    case CardSet.Intrigue:
                    case CardSet.Seaside:
                    case CardSet.Alchemy:
                    case CardSet.Prosperity:
                    case CardSet.Cornucopia:
                    case CardSet.Hinterlands:
                    case CardSet.DarkAges:
                    case CardSet.Guilds:
                        rulesUrl = string.Format("http://dominiongame.info/dominion{0}rules.pdf", set);
                        break;
                    case CardSet.Adventures:
                        rulesUrl = "http://riograndegames.com/getFile.php?id=1907";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("set");
                }

                this.Monitor = new DownloadingTransferMonitor(
                    rulesUrl,
                    string.Format("Dominion{0}Rules.pdf", set),
                    Localized.CardData.GetLocalizedValue(set));
                this.IconPath = string.Format("../Images/SetIcons/{0}.png", set);
            }

            public DownloadingTransferMonitor Monitor { get; set; }
            public string IconPath { get; set; } 
        }

        private async void DominionTransferControl_OnTap(object sender, GestureEventArgs e)
        {
            var transferControl = sender as TransferControl;
            if (transferControl == null)
            {
                throw new ArgumentException("Sender must be a TransferControl", "sender");
            }

            var monitor = transferControl.Monitor as DownloadingTransferMonitor;
            if (monitor == null)
            {
                throw new ArgumentException("TransferControl's Monitor must be a DownloadingTransferMonitor", "sender");
            }

            await monitor.OpenRulesAsync();
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
            await Task.Yield();

            try
            {
                Button b = sender as Button;
            }
            catch (Exception ex)
            {
                AppLog.Instance.Error("Failed to load Rules PDF", ex);
            }
        }


        private void MarketplaceButton_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask dt = new MarketplaceDetailTask();
            dt.ContentIdentifier = sender.GetTag<string>();
            dt.ContentType = MarketplaceContentType.Applications;

            dt.Show();
        }
    }

    public class DownloadingTransferMonitor : TransferMonitor
    {
        private readonly AsyncLazy<IStorageFile> rulesFile;
        private readonly Uri downloadLocation;
        private BackgroundTransferRequest request;

        public DownloadingTransferMonitor(BackgroundTransferRequest request) : this(request, null)
        {
        }

        public DownloadingTransferMonitor(string requestUri, string localUri, string name)
            : this(CreateBackgroundTransferRequest(requestUri, localUri), name)
        {
        }

        private static BackgroundTransferRequest CreateBackgroundTransferRequest(string requestPath, string localPath)
        {
            Uri requestUri = new Uri(requestPath);
            string downloadPath = Path.Combine("shared/transfers", localPath);
            Uri downloadUri = new Uri(downloadPath, UriKind.RelativeOrAbsolute);
            var request = new BackgroundTransferRequest(requestUri, downloadUri) {TransferPreferences = TransferPreferences.AllowCellularAndBattery};

            return request;
        }

        public DownloadingTransferMonitor(BackgroundTransferRequest request, string name) : base(request, name)
        {
            this.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "StatusText" || args.PropertyName == "State")
                {
                    // Whenever StatusText changes we want to notify that
                    // our custom FileStatus property is also changed
                    this.OnPropertyChanged("FileStatus");
                }
            };

            this.request = request;
            this.downloadLocation = request.DownloadLocation;
            this.rulesFile = new AsyncLazy<IStorageFile>((Func<Task<IStorageFile>>)this.GetLocalRulesFileAsync);

            Task.Run(async () =>
            {
                // Kick off the loading of the local file so it's ready when we need it
                await this.rulesFile;
            });
        }

        public string FileStatus
        {
            get
            {
                switch (this.State)
                {
                    case TransferRequestState.Pending:
                        return "Tap to download";
                    case TransferRequestState.Complete:
                        return "Tap to open";
                    case TransferRequestState.Downloading:
                    case TransferRequestState.Uploading:
                    case TransferRequestState.Paused:
                    case TransferRequestState.Waiting:
                    case TransferRequestState.Failed:
                    case TransferRequestState.Unknown:
                    default:
                        return this.StatusText;
                }
            }
        }

        private async Task<IStorageFile> GetLocalRulesFileAsync()
        {
            var folder = ApplicationData.Current.LocalFolder;

            string filePath = this.downloadLocation.ToString().TrimStart('\\');
            IStorageFile file = await folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);

            var properties = await file.GetBasicPropertiesAsync();
            if (properties.Size != 0)
            {
                TaskCompletionSource<object> stateUpdated = new TaskCompletionSource<object>();
                Task dispatcherTask = DispatcherHelper.RunAsync(() =>
                {
                    this.State = TransferRequestState.Complete;
                    stateUpdated.SetResult(null);
                });

                await stateUpdated.Task;
            }

            return file;
        }

        public async Task OpenRulesAsync()
        {
            IStorageFile rules = await this.rulesFile;

            if (this.State == TransferRequestState.Complete)
            {
                if (await Launcher.LaunchFileAsync(rules))
                {
                    AppLog.Instance.Log("Opened file " + rules.Name);
                }
            }
            else if (this.State == TransferRequestState.Pending)
            {
                AppLog.Instance.Log("Downloading file " + rules.Name);
                this.RequestStart();
                //try
                //{
                //    this.request.TransferStatusChanged += (sender, args) =>
                //    {
                //        var error = this.request.TransferError;
                //    };
                //    BackgroundTransferService.Add(this.request);
                //}
                //catch (Exception)
                //{
                //    throw;
                //}
                await Task.Delay(100);
            }
        }
    }
}