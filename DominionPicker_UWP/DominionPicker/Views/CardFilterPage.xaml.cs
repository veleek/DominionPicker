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

    public partial class CardFilterPage
       : Windows.UI.Xaml.Controls.Page
    {

        public CardFilterPage()
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
        /// Reset the list of filtered cards
        /// </summary>
        private void ResetFilteredCards()
        {
            this.ViewModel.ResetFilteredCards();
            this.ClearFilter();
        }

        private void ClearFilter()
        {
            if (this.SearchTextBox.Text == String.Empty)
            {
                // Manually filter on empty
                this.ViewModel.FilterCardsList(String.Empty);
            }
            else
            {
                this.SearchTextBox.Text = String.Empty;
            }
        }

        /// <summary>
        /// An event handler to listen for changes to the search box text and dynamically filter the cards list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.FilterCardsList(this.SearchTextBox.Text);
        }

        /// <summary>
        /// An event handler to listen for keys on the text box to handle closing the filter if you press enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.CardsList.Focus(FocusState.Programmatic);
            }
        }

        /// <summary>
        /// Called when the SearchBox action icon is tapped to clear the current search filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            this.ClearFilter();
        }

        /// <summary>
        /// Called to show only the currently filtered cards in the list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFilteredCards_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.FilterCardsList(CardLookupViewModel.FilteredCardsSeachFilter);
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
                this.ResetFilteredCards();
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