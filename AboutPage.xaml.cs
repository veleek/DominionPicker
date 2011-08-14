using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Ben.Dominion
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(AboutPage_Loaded);
            VersionTextBlock.Text = this.GetType().Assembly.ToString().Split('=', ',')[2];
        }

        void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            App app = App.Current as App;

            //if (app.IsTrial)
            //{
            //    BuyNowButton.Content = "Buy Now";
            //}
        }

        private void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "randall.ben@gmail.com";
            emailComposeTask.Subject = "Dominion Picker";
            emailComposeTask.Body = "Comments:\nRequests:";
            emailComposeTask.Show();
        }

        private void RateButton_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
            reviewTask.Show();
        }

        private void BuyNowButton_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask detailTask = new MarketplaceDetailTask();
            detailTask.ContentType = MarketplaceContentType.Applications;
            detailTask.Show();
        }

        private void TwitterButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask browserTask = new WebBrowserTask();
            browserTask.URL = "http://twitter.com/veleek";
            browserTask.Show();
        }
    }
}
