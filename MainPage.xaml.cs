﻿using System;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using Ben.Utilities;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Data;
using Ben.Phone;

namespace Ben.Dominion
{
    public partial class MainPage : PhoneApplicationPage
    {
        public PickerState CurrentState;
        public ApplicationBarIconButton UnlockButton;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.Unloaded += new RoutedEventHandler(MainPage_Unloaded);
            this.BackKeyPress += new EventHandler<CancelEventArgs>(MainPage_BackKeyPress);

            UnlockButton = new ApplicationBarIconButton(new Uri("/Images/appbar.lock.png", UriKind.Relative));
            UnlockButton.Click += new EventHandler(UnlockButton_Click);
            UnlockButton.Text = "Lock";
            
            LoadState();
        }

        public void LoadState()
        {
            // Check if the state has changed
            if (PickerState.Current != this.CurrentState)
            {
                // If it has, set the appropriate field and data context
                this.DataContext = this.CurrentState = PickerState.Current;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            App app = App.Current as App;
            //AdManager.LoadAd(AdContainer);

            //if (app.IsTrial)
            //{
            //    this.ApplicationBar.Buttons.Add(UnlockButton);    
            //}
            //else
            //{
            //    if (this.ApplicationBar.Buttons.Contains(UnlockButton))
            //    {
            //        this.ApplicationBar.Buttons.Remove(UnlockButton);
            //    }
            //}

            Int32 appLaunchCount = 0;
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.TryGetValue("AppLaunchCount", out appLaunchCount);

            if (appLaunchCount == 10)
            {
                RequestReviewPopup.Visibility = Visibility.Visible;
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AdManager.UnloadAd(AdContainer);
        }

        private void MainPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            PickerState.Current.CancelGeneration();
            if (AddFavoritePopup.IsOpen)
            {
                AddFavoritePopup.IsOpen = false;
                e.Cancel = true;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Start and show the progress bar, disable the create button
            GenerationProgressBar.Visibility = System.Windows.Visibility.Visible;
            GenerationProgressBar.IsIndeterminate = true;
            CreateButton.IsEnabled = false;

            // We don't want the generation to happen on the UI thread.
            // A background worker will enable stuff to continue (e.g.
            // quit the app) while the generation is happening.
            BackgroundWorker generateWorker = new BackgroundWorker();
            generateWorker.DoWork += (backgroundSender, backgroundArgs) =>
            {
                try
                {
                    if (this.CurrentState.GenerateCardList())
                    {
                        // Navigation has to happen on the UI thread, so ask the Dispatcher to do it
                        Dispatcher.BeginInvoke(() =>
                        {
                            this.NavigationService.Navigate(new Uri("/ResultsViewer.xaml", UriKind.Relative));
                        });
                    }
                }
                finally
                {
                    // And finally when everything is done, ask the UI thread to reenable
                    // the buttons and hide the progress bar
                    Dispatcher.BeginInvoke(() =>
                    {
                        CreateButton.IsEnabled = true;
                        GenerationProgressBar.IsIndeterminate = false;
                        GenerationProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                    });
                }
            };

            generateWorker.RunWorkerAsync();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            CurrentState.Reset();
            SettingsScrollViewer.ScrollToVerticalOffset(0);
        }

        private void AddFavorite_Click(object sender, EventArgs e)
        {
            AddFavoritePopup.IsOpen = !AddFavoritePopup.IsOpen;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState.LoadSettings(((FrameworkElement)sender).DataContext as PickerSettings);
            RootPivot.SelectedIndex = 0;
            SettingsScrollViewer.ScrollToVerticalOffset(0);
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            MarketplaceDetailTask detailTask = new MarketplaceDetailTask();
            detailTask.ContentType = MarketplaceContentType.Applications;
            detailTask.Show();
        }
        
        private void ClearFavorites_Click(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("This will delete all your favorites.\nAre you sure you want to continue?", "Warning", MessageBoxButton.OKCancel);

            if (res == MessageBoxResult.OK)
            {
                // Delete all the favorites
                CurrentState.FavoriteSettings.Clear();
                // And delete all the saved state (probably don't need this)
                PickerState.ClearSavedState();

                RootPivot.SelectedIndex = 0;
                SettingsScrollViewer.ScrollToVerticalOffset(0);
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
			this.NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void RequestReviewOk_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask review = new MarketplaceReviewTask();
            review.Show();
            RequestReviewPopup.Visibility = Visibility.Collapsed;
        }

        private void RequestReviewCancel_Click(object sender, RoutedEventArgs e)
        {
            RequestReviewPopup.Visibility = Visibility.Collapsed;
        }
    }
}