using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Ben.Dominion.Resources;
using Ben.Dominion.Utilities;
using Ben.Dominion.Views;
using Ben.Dominion.ViewModels;
using Windows.Foundation;
using Ben.Dominion.Controls;
using System.Windows.Input;

namespace Ben.Dominion
{
    public partial class ResultsViewer : Page
    {
        public ResultsViewer()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) => { MainView.CancelGeneration(); };

            // This resolves the issue with the "The data necessary to complete 
            // this operation is not yet available." exception.  We ignore the 
            // card list items until after it's completed loading, at which point
            // we enable the hit test again.
            /*
            this.GroupedCardsList.IsHitTestVisible = false;
            this.GroupedCardsList.Loaded += (sender, args) =>
               {
                   this.GroupedCardsList.IsHitTestVisible = true;
               };
            */
        }

        public MainViewModel MainView
        {
            get
            {
                return MainViewModel.Instance;
            }
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.UpdateSorting(this.MainView.Result.SortOrder);
            base.OnNavigatedTo(e);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton refreshButton = (AppBarButton)sender;
            try
            {
                // Start and show the progress bar, disable the create button
                //WindowsPhoneUWP.UpgradeHelpers.ProgressIndicator.ChangeVisibility(Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ProgressIndicator, true);
                refreshButton.IsEnabled = false;
                // We don't want the generation to happen on the UI thread.
                // A background worker will enable stuff to continue (e.g.
                // quit the app) while the generation is happening.
                BackgroundWorker generateWorker = new BackgroundWorker();
                generateWorker.DoWork += async (backgroundSender, backgroundArgs) =>
                {
                    try
                    {
                        // If we fail to generate a set, we don't do anything
                        MainView.GenerateCardList();
                    }
                    finally
                    {
                        // And finally when everything is done, ask the UI thread to reenable
                        // the buttons and hide the progress bar
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                refreshButton.IsEnabled = true;
                                //WindowsPhoneUWP.UpgradeHelpers.ProgressIndicator.ChangeVisibility(Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ProgressIndicator, false);
                            }
                        );
                    }
                };
                generateWorker.RunWorkerAsync();
            }
            catch (Exception)
            {
                // If there's a failure, we need to re-enable the buttons.
                refreshButton.IsEnabled = true;
                //WindowsPhoneUWP.UpgradeHelpers.ProgressIndicator.ChangeVisibility(Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ProgressIndicator, false);
                throw;
            }
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            // Set the sort order to the next sort order
            this.MainView.Result.SortOrder = this.MainView.Result.SortOrder.Next();
            // Then update the view.
            this.UpdateSorting(this.MainView.Result.SortOrder);
        }

        private void UpdateSorting(ResultSortOrder sortOrder)
        {
            this.MainView.Result.GroupedCards.First(g => g.Key.Type == CardGroupType.KingdomCard).Sort(sortOrder);
            // The sort button will display the next sort order option
            String nextSort = sortOrder.Next().ToString();
            this.SortButton.Label = Strings.GetString("Results_Sort" + nextSort);
            //this.SortButton.Icon = new BitmapIcon()
            //{
            //    UriSource = new Uri("ms-appx:///Images/appbar.sort." + nextSort + ".png")
            //};
        }

        private async void AddFavorite_Click(object sender, RoutedEventArgs e)
        {
            AddFavoriteDialog addFavoriteDialog = new AddFavoriteDialog();
            addFavoriteDialog.SaveFavorite += AddFavoriteDialog_SaveFavorite;
            await addFavoriteDialog.ShowAsync();
        }

        private void AddFavoriteDialog_SaveFavorite(object sender, FavoriteEventArgs e)
        {
            this.MainView.SaveFavoriteCardSet(e.FavoriteName);
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

        private void SlidableListItem_RightCommandRequested(object sender, EventArgs e)
        {
            Control listItem = sender as Control;
            Card selectedCard = listItem.DataContext as Card;
            if(selectedCard.Group.Type == CardGroupType.KingdomCard)
            {
                this.MainView.Result.Replace(selectedCard);
            }
        }
    }
}