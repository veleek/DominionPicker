using System;
using System.Reflection;
using System.Threading.Tasks;
using Ben.Utilities;
using Windows.ApplicationModel.Email;
using Windows.System;
using Windows.UI.Xaml;

using GestureEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;

namespace Ben.Dominion
{

    public partial class AboutPage
       : Windows.UI.Xaml.Controls.Page
    {

        public AboutPage()
        {
            this.InitializeComponent();
            string version = this.GetType().GetTypeInfo().Assembly.ToString().Split('=', ',')[2];
#if DEBUG
            version += " - DEBUG";
#endif
            this.VersionTextBlock.Text = version;
            //this.RulesInfo.ItemsSource = new[] { new RulesInfoItem(CardSet.Base), new RulesInfoItem(CardSet.Intrigue), new RulesInfoItem(CardSet.Seaside), new RulesInfoItem(CardSet.Alchemy), new RulesInfoItem(CardSet.Prosperity), new RulesInfoItem(CardSet.Cornucopia), new RulesInfoItem(CardSet.Hinterlands), new RulesInfoItem(CardSet.DarkAges), new RulesInfoItem(CardSet.Guilds), new RulesInfoItem(CardSet.Adventures), };
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
                //this.Monitor = new DownloadingTransferMonitor(rulesUrl, string.Format("Dominion{0}Rules.pdf", set), Localized.CardData.GetLocalizedValue(set));
                this.IconPath = string.Format("../Images/SetIcons/{0}.png", set);
            }

            //public DownloadingTransferMonitor Monitor { get; set; }

            public string IconPath { get; set; }

        }

        private void DominionTransferControl_OnTap(object sender, GestureEventArgs e)
        {
            //var transferControl = sender as TransferControl;
            //if ( transferControl == null )
            //{
            //   throw new ArgumentException("Sender must be a TransferControl", "sender");
            //}
            //var monitor = transferControl.Monitor as DownloadingTransferMonitor;
            //if ( monitor == null )
            //{
            //   throw new ArgumentException("TransferControl's Monitor must be a DownloadingTransferMonitor", "sender");
            //}
            //await monitor.OpenRulesAsync();
        }

        private async void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage emailComposeTask = new EmailMessage()
            {
                To = { new EmailRecipient("randall.ben@gmail.com", "Ben Randall") },
                Subject = "Dominion Picker",
                Body = "Comments:\nRequests:\n",
            };
            await EmailManager.ShowComposeNewEmailAsync(emailComposeTask);
        }

        private async void RateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9WZDNCRDM8J9"));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?ProductId=9WZDNCRDM8J9"));
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
                Windows.UI.Xaml.Controls.Button b = sender as Windows.UI.Xaml.Controls.Button;
            }
            catch (Exception ex)
            {
                AppLog.Instance.Error("Failed to load Rules PDF", ex);
            }
        }

        private async void MarketplaceButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:navigate?appid=" + sender.GetTag<string>() + "&contenttype=" + "app"));
        }

    }
}