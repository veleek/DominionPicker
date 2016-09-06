using System;
using Ben.Dominion.Resources;
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
            var resetFilteredMenuItem = new AppBarButton
            {
                Label = Strings.Lookup_ResetFiltered
            };
            resetFilteredMenuItem.Click += this.ResetFilteredCards_Click;
            ((CommandBar)BottomAppBar).SecondaryCommands.Add(resetFilteredMenuItem);
            var showFilteredMenuItem = new AppBarButton
            {
                Label = Strings.Lookup_ShowFiltered
            };
            showFilteredMenuItem.Click += this.ShowFilteredCards_Click;
            ((CommandBar)BottomAppBar).SecondaryCommands.Add(showFilteredMenuItem);
            var aboutMenuItem = new AppBarButton
            {
                Label = Strings.Menu_About
            };
            aboutMenuItem.Click += this.About_Click;
            ((CommandBar)BottomAppBar).SecondaryCommands.Add(aboutMenuItem);
        }

        public CardLookupViewModel ViewModel
        {
            get
            {
                return CardLookupViewModel.Instance;
            }
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.ViewModel.LoadFilteredCards();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.ViewModel.SaveFilteredCards();
        }

        /// <summary>
        /// Called to show only the currently filtered cards in the list box
        /// </summary>
        private void ShowFilteredCards_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.SearchText = CardLookupViewModel.FilteredCardsSeachFilter;
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

        private void CardItemDetails_Click(object sender, RoutedEventArgs e)
        {
            App.Instance.SelectedCard = sender.GetContext<CardSelector>().Card;
            PickerView.CardInfo.Go();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            PickerView.About.Go();
        }
    }
}