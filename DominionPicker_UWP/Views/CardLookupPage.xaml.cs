using System;
using Ben.Dominion.Resources;
using Ben.Dominion.Utilities;
using Ben.Dominion.ViewModels;
using Ben.Dominion.Views;
using Ben.Utilities;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ben.Dominion
{
    public partial class CardLookupPage : Page
    {
        public CardLookupPage()
        {
            this.InitializeComponent();
        }

        public CardLookupViewModel ViewModel
        {
            get { return CardLookupViewModel.Instance; }
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.ViewModel.LoadFilteredCards();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.ViewModel.SaveFilteredCards();
        }

        private async void ResetFilteredCards_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog resetDialog = new MessageDialog("This will clear all cards you have previously chosen to filter.  Do you want to continue?", "Warning!")
            {
                Commands =
                  {
                     new UICommand { Id = 10, Label = "OK" },
                     new UICommand { Id = 20, Label = "Cancel" }
                  }
            };

            IUICommand result = await resetDialog.ShowAsync();
            if (result.Label == "OK")
            {
                this.ViewModel.ResetFilteredCards();
            }
        }
    }
}