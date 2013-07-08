using System;
using System.ComponentModel;
using System.Windows;
using Ben.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls;

namespace Ben.Dominion
{
    public partial class ResultsViewer : PhoneApplicationPage
    {
        public ResultsViewer()
        {
            InitializeComponent();

            this.BackKeyPress += new EventHandler<CancelEventArgs>(ResultsViewer_BackKeyPress);

            // This resolves the issue with the "The data necessary to complete 
            // this operation is not yet available." exception.  We ignore the 
            // card list items until after it's completed loading, at which point
            // we enable the hit test again.
            CardsList.IsHitTestVisible = false;
            CardsList.Loaded += new RoutedEventHandler(CardsList_Loaded);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateSortButton(MainViewModel.Instance.Result.SortOrder);

            base.OnNavigatedTo(e);
        }

        void CardsList_Loaded(object sender, RoutedEventArgs e)
        {
            CardsList.IsHitTestVisible = true;
        }

        void ResultsViewer_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AddFavoritePopup.IsOpen)
            {
                AddFavoritePopup.IsOpen = false;
                e.Cancel = true;
            }
            else
            {
                MainViewModel.Instance.CancelGeneration();
            }
        }

        private void CardItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            (App.Current as App).SelectedCard = sender.GetContext<Card>();
            NavigationService.Navigate("/CardInfo.xaml");
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            // Start and show the progress bar, disable the create button
            SystemTray.ProgressIndicator.IsVisible = true;
            ApplicationBarIconButton refreshButton = sender as ApplicationBarIconButton;
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
                    Dispatcher.BeginInvoke(() =>
                    {
                        refreshButton.IsEnabled = true;
                        SystemTray.ProgressIndicator.IsVisible = false;
                    });
                }
            };

            generateWorker.RunWorkerAsync();
        }

        private void Sort_Click(object sender, EventArgs e)
        {
            ResultSortOrder sortOrder = NextSortOrder(MainViewModel.Instance.Result.SortOrder);
            MainViewModel.Instance.Result.Sort(sortOrder);
            UpdateSortButton(sortOrder);
        }

        private void UpdateSortButton(ResultSortOrder sortOrder)
        {
            var sortButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            String nextSort = NextSortOrder(sortOrder).ToString().ToLower();
            sortButton.Text = "sort " + nextSort;
            sortButton.IconUri = new Uri("/images/appbar.sort." + nextSort + ".png", UriKind.Relative);
        }

        public static ResultSortOrder NextSortOrder(ResultSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case ResultSortOrder.Name:
                    return ResultSortOrder.Cost;
                case ResultSortOrder.Cost:
                    return ResultSortOrder.Set;
                case ResultSortOrder.Set:
                    return ResultSortOrder.Name;
                default:
                    return ResultSortOrder.Name;
            }
        }

        private void AddFavorite_Click(object sender, EventArgs e)
        {
            AddFavoritePopup.IsOpen = !AddFavoritePopup.IsOpen;
        }

        private void AddFavoritePopup_SaveFavorite(object sender, FavoriteEventArgs e)
        {
            SaveFavoriteCardSet(e.FavoriteName);
        }

        public void SaveFavoriteCardSet(String name)
        {
            MainViewModel.Instance.SaveFavoriteCardSet(name);
        }

        private void ScrollViewer_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }

            var v = e.FinalVelocities.LinearVelocity;
            var m = v.GetMagnitude();
            var a = Math.Abs(v.X);

            // If it's an intertal manipulation, and it's 80% in the X direction
            // and the total manipulitation is greater than 400
            if (e.IsInertial && (a / m) > 0.8 && m > 400)
            {
                MainViewModel.Instance.Result.Replace(scrollViewer.DataContext as Card);
            }
        }

        private void FilterCards_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate("/CardFilterPage.xaml");
        }

        private void About_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate("/AboutPage.xaml");
        }
    }
}
