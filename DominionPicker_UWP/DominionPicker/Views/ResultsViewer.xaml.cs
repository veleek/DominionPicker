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
using Ben.Dominion.TestControls;

namespace Ben.Dominion
{

    public partial class ResultsViewer : Page
    {
        private readonly AppBarButton SortButton;

        public ResultsViewer()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += this.ResultsViewer_BackKeyPress;

            // This resolves the issue with the "The data necessary to complete 
            // this operation is not yet available." exception.  We ignore the 
            // card list items until after it's completed loading, at which point
            // we enable the hit test again.
            this.GroupedCardsList.IsHitTestVisible = false;
            this.GroupedCardsList.Loaded += (sender, args) =>
               {
                   this.GroupedCardsList.IsHitTestVisible = true;
               };

            var refreshButton = new AppBarButton();
            refreshButton.Click += this.Refresh_Click;

            ((CommandBar)BottomAppBar).PrimaryCommands.Add(refreshButton);
            this.SortButton = ApplicationBarHelper.CreateIconButton(Strings.Results_SortName, @"/Images/appbar.sort.name.png", this.Sort_Click);
            ((CommandBar)BottomAppBar).PrimaryCommands.Add(this.SortButton);
            ((CommandBar)BottomAppBar).AddIconButton(Strings.Results_Save, @"/Images/appbar.favs.addto.png", this.AddFavorite_Click);
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_CardLookup, this.CardLookup_Click);
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_BlackMarket, this.BlackMarket_Click);
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_Settings, this.Settings_Click);
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_About, this.About_Click);
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
            this.UpdateSorting(MainViewModel.Instance.Result.SortOrder);
            base.OnNavigatedTo(e);
        }

        private void ResultsViewer_BackKeyPress(object sender, BackRequestedEventArgs e)
        {
            if (this.AddFavoritePopup.IsOpen)
            {
                this.AddFavoritePopup.IsOpen = false;
                e.Handled = true;
            }
            else
            {
                MainViewModel.Instance.CancelGeneration();
            }
        }

        private void CardItem_Tap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DominionCardControl cardControl = sender as DominionCardControl;
            App.Instance.SelectedCard = cardControl.Card;
            PickerView.CardInfo.Go();
        }

        private void CardItem_Swipe(object sender, RoutedEventArgs e)
        {
            DominionCardControl cardControl = sender as DominionCardControl;
            if (cardControl.Card.Group.Type == CardGroupType.KingdomCard)
            {
                MainViewModel.Instance.Result.Replace(cardControl.Card);
            }
        }

        private void DominionCardControl_Swipe(object sender, EventArgs e)
        {
            DominionCardControl cardControl = sender as DominionCardControl;
            if (cardControl.Card.Group.Type == CardGroupType.KingdomCard)
            {
                MainViewModel.Instance.Result.Replace(cardControl.Card);
            }
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
                       IAsyncAction updateProgressBarTask = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                          {
                                refreshButton.IsEnabled = true;
                            //WindowsPhoneUWP.UpgradeHelpers.ProgressIndicator.ChangeVisibility(Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ProgressIndicator, false);
                        });
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
            MainViewModel.Instance.Result.SortOrder = MainViewModel.Instance.Result.SortOrder.Next();
            // Then update the view.
            this.UpdateSorting(MainViewModel.Instance.Result.SortOrder);
        }

        private void UpdateSorting(ResultSortOrder sortOrder)
        {
            this.MainView.Result.GroupedCards.First(g => g.Key.Type == CardGroupType.KingdomCard).Sort(sortOrder);
            // The sort button will display the next sort order option
            String nextSort = sortOrder.Next().ToString();
            this.SortButton.Label = Strings.GetString("Results_Sort" + nextSort);
            this.SortButton.Icon = new Windows.UI.Xaml.Controls.BitmapIcon()
            {
                UriSource = new Uri("ms-appx://images/appbar.sort." + nextSort + ".png")
            };
        }

        private void AddFavorite_Click(object sender, RoutedEventArgs e)
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

        private void CardLookup_Click(object sender, RoutedEventArgs e)
        {
            PickerView.CardFilter.Go();
        }

        private void BlackMarket_Click(object sender, RoutedEventArgs e)
        {
            PickerView.BlackMarket.Go();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            PickerView.Settings.Go();
        }

        private void About_Click(object sender, RoutedEventArgs e)
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