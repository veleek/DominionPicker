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