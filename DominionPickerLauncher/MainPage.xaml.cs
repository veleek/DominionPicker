using System;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace DominionPickerLauncher
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string DominionPickerAppId = "1d2db065-f908-e011-9264-00237de2db9e";

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        public Package Package => Package.Current;

        public string VersionString => string.Format(
            "{0}.{1}.{2}.{3}",
            this.Package.Id.Version.Major,
            this.Package.Id.Version.Minor,
            this.Package.Id.Version.Build,
            this.Package.Id.Version.Revision);

        private async void DownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            var downloadUri = new Uri(string.Format("ms-windows-store:navigate?appid={0}", DominionPickerAppId));
            await Launcher.LaunchUriAsync(downloadUri);
        }

        private async void LaunchAnywayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!PickerLauncher.TryLaunch(false))
            {
                MessageDialog launchFailedDialog =
                    new MessageDialog(
                        "Unable to launch Dominion Picker.  Make sure the most recent update is installed.",
                        "Launch failed");
                await launchFailedDialog.ShowAsync();
            }
        }
    }
}