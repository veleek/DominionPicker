using System;
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
        private PickerState CurrentState { get { return PickerState.Current; } }
        private ApplicationBarIconButton UnlockButton;
        private Boolean reviewRequestShown = false;
        private Boolean isNew = false;
        private Object favoriteEdit = null;

        private ApplicationBarIconButton ResetButton;
        private ApplicationBarIconButton AddFavoriteButton;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            isNew = true;

            SolidColorBrush backgroundBrush = RequestReviewPopup.Background as SolidColorBrush;
            Color backgroundColor = backgroundBrush.Color;
            backgroundColor.A = 0x66;
            backgroundBrush.Color = backgroundColor;

            // Set the data context of the listbox control to the sample data
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.Unloaded += new RoutedEventHandler(MainPage_Unloaded);
            this.BackKeyPress += new EventHandler<CancelEventArgs>(MainPage_BackKeyPress);

            UnlockButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Images/appbar.lock.png", UriKind.Relative),
                Text = "lock",
            };
            UnlockButton.Click += UnlockButton_Click;

            ResetButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Images/appbar.reset.png", UriKind.Relative),
                Text = "reset",
            };
            ResetButton.Click += Reset_Click;

            AddFavoriteButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Images/appbar.favs.addto.png", UriKind.Relative),
                Text = "save fav...",
            };
            AddFavoriteButton.Click += AddFavorite_Click;
            
            LoadState();
        }

        public void LoadState()
        {
            // If it has, set the appropriate field and data context
            this.DataContext = this.CurrentState;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //AdManager.LoadAd(AdContainer);

            //App app = App.Current as App;
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

            if (appLaunchCount == 10 && !reviewRequestShown)
            {
                RequestReviewPopup.Visibility = Visibility.Visible;
                // This prevents us from showing the popup if they 
                // come back here without exiting the app.
                reviewRequestShown = true;
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
                HideAddFavoritePopup();
                e.Cancel = true;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Create();
        }

        private void Create()
        {
            if (this.CurrentState.IsGenerating)
            {
                StopGeneration();
            }
            else
            {
                StartGeneration();
            }
        }

        private void StartGeneration()
        {
            // Start and show the progress bar, and change the create button
            //SystemTray.ProgressIndicator.IsVisible = true;
            CreateButton.Content = "Cancel";

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
                    else
                    {
                        //Dispatcher.BeginInvoke(() =>
                        //{
                        //    MessageBox.Show("Whoops! We couldn't generate a set with those options.");
                        //});
                    }
                }
                finally
                {
                    // And finally when everything is done, ask the UI thread to reenable
                    // the buttons and hide the progress bar
                    Dispatcher.BeginInvoke(() =>
                    {
                        StopGeneration();
                    });
                }
            };

            generateWorker.RunWorkerAsync();
        }

        private void StopGeneration()
        {
            // Stop the generation 
            this.CurrentState.CancelGeneration();

            // Hide the progress bar and swap the button
            //SystemTray.ProgressIndicator.IsVisible = false;
            CreateButton.Content = "Generate";
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            CurrentState.Reset();
            SettingsScrollViewer.ScrollToVerticalOffset(0);
        }

        private void AddFavorite_Click(object sender, EventArgs e)
        {
            if (!AddFavoritePopup.IsOpen)
            {
                favoriteEdit = null;
                AddFavoritePopup.FavoriteName = String.Empty;
                ShowAddFavoritePopup();
            }
        }

        public void ShowAddFavoritePopup()
        {
            AddFavoritePopup.IsOpen = true;
            ResetButton.IsEnabled = false;
            AddFavoriteButton.IsEnabled = false;
        }

        public void HideAddFavoritePopup()
        {
            AddFavoritePopup.IsOpen = false;
            ResetButton.IsEnabled = true;
            AddFavoriteButton.IsEnabled = true;
        }

        private void FavoriteSettingsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                // Load the settings
                CurrentState.LoadSettings((e.AddedItems[0] as FavoriteSetting).Value);
                FavoriteSettingsListBox.SelectedItem = null;

                Create();
            }
        }

        private void FavoriteSetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                // Just in case;
                this.CurrentState.CancelGeneration();

                CurrentState.Result = (e.AddedItems[0] as FavoriteSet).Result;
                this.NavigationService.Navigate(new Uri("/ResultsViewer.xaml", UriKind.Relative));

                // Clear the selection
                FavoriteSetsListBox.SelectedItem = null;
            }
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            MarketplaceDetailTask detailTask = new MarketplaceDetailTask();
            detailTask.ContentType = MarketplaceContentType.Applications;
            detailTask.Show();
        }

        private void AddFavoritePopup_SaveFavorite(object sender, FavoriteEventArgs e)
        {
            if (favoriteEdit == null)
            {
                SaveFavoriteSettings(e.FavoriteName);
                RootPivot.SelectedIndex = 1;
            }
            else if (favoriteEdit is FavoriteSet)
            {
                (favoriteEdit as FavoriteSet).Name = e.FavoriteName;
            }
            else if (favoriteEdit is FavoriteSetting)
            {
                (favoriteEdit as FavoriteSetting).Name = e.FavoriteName;
            }

            HideAddFavoritePopup();
        }

        private void SaveFavoriteSettings(String name)
        {
            CurrentState.SaveFavoriteSettings(name);
        }

        private void ClearFavorites_Click(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("This will delete all your favorites and reset all the settings. \nAre you sure you want to continue?", "Warning", MessageBoxButton.OKCancel);

            if (res == MessageBoxResult.OK)
            {
                // Delete all the saved state on the disk
                // and load the default everything
                PickerState.ClearSavedState();
                // Then load it back into the UI
                LoadState();

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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (isNew)
            {
                // Load the state
            }

            isNew = false;
            base.OnNavigatedTo(e);
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;

            FavoriteSet favSet = fe.DataContext as FavoriteSet;
            if (favSet != null)
            {
                AddFavoritePopup.FavoriteName = favSet.Name;
                favoriteEdit = favSet;
            }
            else
            {
                FavoriteSetting favSetting = fe.DataContext as FavoriteSetting;
                if (favSetting != null)
                {
                    AddFavoritePopup.FavoriteName = favSetting.Name;
                    favoriteEdit = favSetting;
                }
            }

            // Allow this event to complete, and then open the popup after
            // that so that we don't steal the focus away from the textbox.
            // *sigh* Yes, it could have been better to just use a separate 
            // page for the name dialog.
            Dispatcher.BeginInvoke(() => { ShowAddFavoritePopup(); });
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;

            FavoriteSetting favSetting = fe.DataContext as FavoriteSetting;
            if (favSetting != null)
            {
                // Note that we don't call LoadSettings because we explicitly
                // want to modify the settings if we select this option
                CurrentState.CurrentSettings = favSetting.Value;

                // Navigate back to the settings pivot and scroll to the top
                RootPivot.SelectedIndex = 0;
                SettingsScrollViewer.ScrollToVerticalOffset(0);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;

            FavoriteSet favSet = fe.DataContext as FavoriteSet;
            if (favSet != null)
            {
                CurrentState.FavoriteSets.Remove(favSet);
            }
            else
            {
                FavoriteSetting favSetting = fe.DataContext as FavoriteSetting;
                if (favSetting != null)
                {
                    CurrentState.FavoriteSettings.Remove(favSetting);
                }
            }
        }

        private void RootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Boolean onMainPivot = RootPivot.SelectedIndex == 0;

            if (onMainPivot)
            {
                this.ApplicationBar.Buttons.Add(ResetButton);
                this.ApplicationBar.Buttons.Add(AddFavoriteButton);
                this.ApplicationBar.Mode = ApplicationBarMode.Default;
            }
            else
            {
                //this.ApplicationBar.Buttons.Remove(ResetButton);
                //this.ApplicationBar.Buttons.Remove(AddFavoriteButton);
                this.ApplicationBar.Buttons.Clear();
                //this.ApplicationBar.Mode = ApplicationBarMode.Minimized;
            }
        }
    }
}