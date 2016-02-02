using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Windows.UI.Core;
using Ben.Dominion.Resources;
using Ben.Dominion.Utilities;
using Ben.Dominion.Views;
using Ben.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using Ben.Dominion.ViewModels;
using System.Windows.Controls;

namespace Ben.Dominion
{
    public partial class ResultsViewer : PhoneApplicationPage
    {
        private readonly ApplicationBarIconButton SortButton;

        public ResultsViewer()
        {
            this.InitializeComponent();

            this.BackKeyPress += this.ResultsViewer_BackKeyPress;

            // This resolves the issue with the "The data necessary to complete 
            // this operation is not yet available." exception.  We ignore the 
            // card list items until after it's completed loading, at which point
            // we enable the hit test again.
            this.GroupedCardsList.IsHitTestVisible = false;
            this.GroupedCardsList.Loaded += (sender, args) => { this.GroupedCardsList.IsHitTestVisible = true; };

            var refreshButton = new ApplicationBarIconButton
            {
                IconUri = new Uri(@"/Images/appbar.refresh.png", UriKind.Relative),
                Text = Strings.Results_Regenerate,
            };
            refreshButton.Click += this.Refresh_Click;
            this.ApplicationBar.Buttons.Add(refreshButton);

            this.SortButton = ApplicationBarHelper.CreateIconButton(
                Strings.Results_SortName,
                @"/Images/appbar.sort.name.png",
                this.Sort_Click);
            this.ApplicationBar.Buttons.Add(this.SortButton);

            this.ApplicationBar.AddIconButton(
                Strings.Results_Save,
                @"/Images/appbar.favs.addto.png",
                this.AddFavorite_Click);

            // Create all the menu items
            this.ApplicationBar.AddMenuItem(Strings.Menu_CardLookup, this.CardLookup_Click);
            this.ApplicationBar.AddMenuItem(Strings.Menu_BlackMarket, this.BlackMarket_Click);
            this.ApplicationBar.AddMenuItem(Strings.Menu_Settings, this.Settings_Click);
            this.ApplicationBar.AddMenuItem(Strings.Menu_About, this.About_Click);
        }

        public MainViewModel MainView => MainViewModel.Instance;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.UpdateSorting(MainViewModel.Instance.Result.SortOrder);

            base.OnNavigatedTo(e);
        }

        private void ResultsViewer_BackKeyPress(object sender, CancelEventArgs e)
        {
            if (this.AddFavoritePopup.IsOpen)
            {
                this.AddFavoritePopup.IsOpen = false;
                e.Cancel = true;
            }
            else
            {
                MainViewModel.Instance.CancelGeneration();
            }
        }

        private void CardItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DominionCardControl cardControl = sender as DominionCardControl;
            App.Instance.SelectedCard = cardControl.Card;
            PickerView.CardInfo.Go();
        }

        private void CardItem_Swipe(object sender, EventArgs e)
        {
            DominionCardControl cardControl = sender as DominionCardControl;

            if (cardControl.Card.Group.Type == CardGroupType.KingdomCard)
            {
                MainViewModel.Instance.Result.Replace(cardControl.Card);
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton refreshButton = (ApplicationBarIconButton)sender;
            
            try
            {
                // Start and show the progress bar, disable the create button
                SystemTray.ProgressIndicator.IsVisible = true;

                refreshButton.IsEnabled = false;

                // We don't want the generation to happen on the UI thread.
                // A background worker will enable stuff to continue (e.g.
                // quit the app) while the generation is happening.
                BackgroundWorker generateWorker = new BackgroundWorker();
                generateWorker.DoWork += (backgroundSender, backgroundArgs) =>
                {
                    try
                    {
                        // If we fail to generate a set, we don't do anything
                        MainViewModel.Instance.GenerateCardList();
                    }
                    finally
                    {
                        // And finally when everything is done, ask the UI thread to reenable
                        // the buttons and hide the progress bar
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            refreshButton.IsEnabled = true;
                            SystemTray.ProgressIndicator.IsVisible = false;
                        });
                    }
                };

                generateWorker.RunWorkerAsync();
            }
            catch (Exception)
            {
                // If there's a failure, we need to re-enable the buttons.
                refreshButton.IsEnabled = true;
                SystemTray.ProgressIndicator.IsVisible = false;
                throw;
            }
        }

        private void Sort_Click(object sender, EventArgs e)
        {
            // Set the sort order to the next sort order
            MainViewModel.Instance.Result.SortOrder = MainViewModel.Instance.Result.SortOrder.Next();

            // Then update the view.
            this.UpdateSorting(MainViewModel.Instance.Result.SortOrder);
        }

        private void UpdateSorting(ResultSortOrder sortOrder)
        {
            /*
            this.ResultsViewSource.SortDescriptions.Clear();

            if (sortOrder != ResultSortOrder.Name)
            {
                this.ResultsViewSource.SortDescriptions.Add(
                    new SortDescription(
                        sortOrder.ToString(),
                        ListSortDirection.Ascending)
                    );
            }

            // We always sort by DisplayName after we sort by whatever sort property we have.
            this.ResultsViewSource.SortDescriptions.Add(
                new SortDescription(
                    "DisplayName",
                    ListSortDirection.Ascending)
                );
            */

            // The sort button will display the next sort order option
            String nextSort = sortOrder.Next().ToString();
            this.SortButton.Text = Strings.ResourceManager.GetString("Results_Sort" + nextSort, Strings.Culture);
            this.SortButton.IconUri = new Uri("/images/appbar.sort." + nextSort + ".png", UriKind.Relative);
        }

        private void AddFavorite_Click(object sender, EventArgs e)
        {
            this.AddFavoritePopup.IsOpen = !this.AddFavoritePopup.IsOpen;
        }

        private void AddFavoritePopup_SaveFavorite(object sender, FavoriteEventArgs e)
        {
            this.SaveFavoriteCardSet(e.FavoriteName);
        }

        public void SaveFavoriteCardSet(String name)
        {
            MainViewModel.Instance.SaveFavoriteCardSet(name);
        }

        private void CardLookup_Click(object sender, EventArgs e)
        {
            PickerView.CardFilter.Go();
        }

        private void BlackMarket_Click(object sender, EventArgs e)
        {
            PickerView.BlackMarket.Go();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            PickerView.Settings.Go();
        }

        private void About_Click(object sender, EventArgs e)
        {
            PickerView.About.Go();
        }

        private void PlayersButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;
            string player = button.Content.ToString();

            switch (player)
            {
                case "2":
                case "3":
                default:
                    break;
            }
        }
    }
}