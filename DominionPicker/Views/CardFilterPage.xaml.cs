using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ben.Data;
using Ben.Dominion.Resources;
using Ben.Dominion.ViewModels;
using Ben.Dominion.Views;
using Ben.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Ben.Dominion
{
    public partial class CardFilterPage : PhoneApplicationPage
    {
        public CardFilterPage()
        {
            this.InitializeComponent();

            var resetFilteredMenuItem = new ApplicationBarMenuItem {Text = Strings.Lookup_ResetFiltered};
            resetFilteredMenuItem.Click += this.ResetFilteredCards_Click;
            this.ApplicationBar.MenuItems.Add(resetFilteredMenuItem);

            var showFilteredMenuItem = new ApplicationBarMenuItem {Text = Strings.Lookup_ShowFiltered};
            showFilteredMenuItem.Click += this.ShowFilteredCards_Click;
            this.ApplicationBar.MenuItems.Add(showFilteredMenuItem);

            var aboutMenuItem = new ApplicationBarMenuItem {Text = Strings.Menu_About};
            aboutMenuItem.Click += this.About_Click;
            this.ApplicationBar.MenuItems.Add(aboutMenuItem);
        }

        public CardLookupViewModel ViewModel { get { return CardLookupViewModel.Instance; } }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.ViewModel.LoadFilteredCards();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
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

        private async void ClearFilter()
        {
            if (this.SearchTextBox.Text == String.Empty)
            {
                // Manually filter on empty
                await this.ViewModel.FilterCardsListAsync(String.Empty);
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
        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await this.ViewModel.FilterCardsListAsync(this.SearchTextBox.Text);
        }

        /// <summary>
        /// An event handler to listen for keys on the text box to handle closing the filter if you press enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.CardsList.Focus();
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
        private async void ShowFilteredCards_Click(object sender, EventArgs e)
        {
            await this.ViewModel.FilterCardsListAsync(CardLookupViewModel.FilteredCardsSeachFilter);
        }

        private void ResetFilteredCards_Click(object sender, EventArgs e)
        {
            var result =
                MessageBox.Show(
                    "This will clear all cards you have previously chosen to filter.  Do you want to continue?",
                    "Warning!", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.ResetFilteredCards();
            }
        }

        private void CardItemDetails_Click(object sender, RoutedEventArgs e)
        {
            App.Instance.SelectedCard = sender.GetContext<CardSelector>().Card;
            PickerView.CardInfo.Go();
        }

        private void About_Click(object sender, EventArgs e)
        {
            PickerView.About.Go();
        }
    }
}