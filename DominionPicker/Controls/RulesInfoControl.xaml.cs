using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Storage;
using Windows.System;
using Ben.Dominion.Utilities;
using Ben.Utilities;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Ben.Dominion.Controls
{
    public partial class RulesInfoControl : UserControl
    {
        public RulesInfoControl()
        {
            this.InitializeComponent();
        }


        public RulesInfoViewModel RulesInfo
        {
            get { return (RulesInfoViewModel) this.GetValue(RulesInfoProperty); }
            set { this.SetValue(RulesInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RulesInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RulesInfoProperty =
            DependencyProperty.Register("RulesInfo", typeof (RulesInfoViewModel), typeof (RulesInfoControl), new PropertyMetadata(null));

        private void LayoutRoot_OnTap(object sender, GestureEventArgs e)
        {
        }
    }

    public class RulesInfoViewModel : NotifyPropertyChangedBase
    {
        private readonly AsyncLazy<IStorageFile> rulesFile;
        private readonly string localFileName;
        private string status;
        private bool needDownload;

        public RulesInfoViewModel()
        {
            this.Set = CardSet.DarkAges;
            this.Status = "Tap to download";
            this.Progress = 75;
        }

        public RulesInfoViewModel(CardSet set, string rulesUri) : this()
        {
            if (set == CardSet.None)
            {
                throw new ArgumentException("You must specify a valid set for the Rules Info View");
            }

            if (rulesUri == null)
            {
                throw new ArgumentNullException("rulesUri");
            }

            this.Set = set;
            this.RulesUri = rulesUri;
            this.localFileName = Path.Combine(@"shared\transfers", Path.GetFileName(rulesUri));

            this.rulesFile = new AsyncLazy<IStorageFile>((Func<Task<IStorageFile>>) this.GetLocalRulesFileAsync);

            this.Status = "Tap to download rules";
            this.NeedDownload = true;

            Task.Run(async () =>
            {
                // Kick off the loading of the local file so it's ready when we need it
                await this.rulesFile;
            });
        }

        public CardSetViewModel Set { get; set; }

        public string RulesUri { get; }

        public string Header => this.Set.DisplayName + " Rules";

        public string Status
        {
            get { return this.status; }
            set { this.SetProperty(ref this.status, value); }
        }

        public bool NeedDownload
        {
            get { return this.needDownload; }
            private set { this.SetProperty(ref this.needDownload, value); }
        }

        public bool Downloading { get; set; }

        public int Progress { get; set; }

        private async Task<IStorageFile> GetLocalRulesFileAsync()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;

                IStorageFile file = await folder.CreateFileAsync(this.localFileName, CreationCollisionOption.OpenIfExists);

                var properties = await file.GetBasicPropertiesAsync();
                if (properties.Size != 0)
                {
                    this.NeedDownload = false;
                    this.Status = "Tap to open rules";
                }

                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task DownloadRulesFileAsync()
        {
            await Task.Yield();

            Uri requestUri = new Uri(this.RulesUri);
            Uri downloadUri = new Uri(this.localFileName, UriKind.RelativeOrAbsolute);
            BackgroundTransferRequest req = new BackgroundTransferRequest(requestUri, downloadUri)
            {
                TransferPreferences = TransferPreferences.AllowCellularAndBattery
            };

            BackgroundTransferService.Add(req);

            TransferMonitor monitor = new TransferMonitor(req);
            monitor.Complete += (sender, args) => BackgroundTransferService.Remove(args.Request);
        }

        public async Task<IStorageFile> DownloadRulesFileAsync2()
        {
            IStorageFile file = await this.rulesFile;
            AppLog.Instance.Debug("Unable to find local rules PDF for {0}. Downloading from {1}", this.localFileName,
                this.RulesUri);
            // The file wasn't found so let's download the file.
            using (HttpClient client = new HttpClient())
            {
                Stream rulesFileStream = await client.GetStreamAsync(this.RulesUri);

                using (var storageFileStream = await file.OpenStreamForWriteAsync())
                {
                    await rulesFileStream.CopyToAsync(storageFileStream);

                    AppLog.Instance.Debug("Finished downloading rules file.");
                }
            }

            return file;
        }

        public async Task OpenRulesAsync()
        {
            IStorageFile rules = await this.rulesFile;
            if (await Launcher.LaunchFileAsync(rules))
            {
                AppLog.Instance.Log("Opened Rules PDF " + rules.Name);
            }
        }

    }
}