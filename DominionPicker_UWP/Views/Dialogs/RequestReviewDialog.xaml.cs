using System;
using Windows.UI.Xaml.Controls;

namespace Ben.Dominion.Views
{
    public sealed partial class RequestReviewDialog : ContentDialog
    {
        public RequestReviewDialog()
        {
            this.InitializeComponent();
        }

        private async void RequestReviewOk_Click(ContentDialog dialog, ContentDialogButtonClickEventArgs args)
        {
            //this.Hide();
            var deferral = args.GetDeferral();
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + Windows.ApplicationModel.Package.Current.Id.Name));
            deferral.Complete();
        }

        private void RequestReviewCancel_Click(ContentDialog dialog, ContentDialogButtonClickEventArgs args)
        {
            //this.Hide();
        }
    }
}
