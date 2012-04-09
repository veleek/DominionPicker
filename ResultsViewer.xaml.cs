using System;
using System.Windows;
using Ben.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;

namespace Ben.Dominion
{
    public partial class ResultsViewer : PhoneApplicationPage
    {
        public ResultsViewer()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ResultsViewer_Loaded);
            this.BackKeyPress += new EventHandler<CancelEventArgs>(ResultsViewer_BackKeyPress);
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
                PickerState.Current.CancelGeneration();
            }
        }

        void ResultsViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = PickerState.Current;
            UpdateSortButton(PickerState.Current.SortOrder);
        }

        private void CardItem_Flick(object sender, FlickGestureEventArgs e)
        {
            //if (AddFavoritePopup.IsOpen)
            //{
            //    return;
            //}

            //if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            //{
            //    FrameworkElement element = sender as FrameworkElement;
            //    if (element == null)
            //    {
            //        return;
            //    }

            //    PickerState.Current.ReplaceCard(element.DataContext as Card);
            //}
        }

        private void CardItem_Tap(object sender, GestureEventArgs e)
        {
            if (AddFavoritePopup.IsOpen)
            {
                return;
            }

            (App.Current as App).SelectedCard = sender.GetContext<Card>();
            NavigationService.Navigate(new Uri("/CardInfo.xaml", UriKind.Relative));
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
                    PickerState.Current.GenerateCardList();
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
            ResultSortOrder sortOrder = NextSortOrder(PickerState.Current.SortOrder);
            PickerState.Current.SortBy(sortOrder);
            UpdateSortButton(sortOrder);
        }

        private void UpdateSortButton(ResultSortOrder sortOrder)
        {
            var sortButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            String nextSort = NextSortOrder(sortOrder).ToString().ToLower();
            sortButton.Text = "sort by " + nextSort;
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
            PickerState.Current.SaveFavoriteCardSet(name);
        }

        private void ScrollViewer_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void ScrollViewer_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            var v = e.FinalVelocities.LinearVelocity;
            var m = v.GetMagnitude();
            var a = Math.Abs(v.X);

            if (e.IsInertial && (a / m) > 0.8)
            {
                var scrollViewer = sender as System.Windows.Controls.ScrollViewer;
                if (scrollViewer == null)
                {
                    return;
                }

                //MessageBox.Show(String.Format("H: {0}, V: {1}", scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset));

                PickerState.Current.ReplaceCard(scrollViewer.DataContext as Card);
            }
        }
    }
}
