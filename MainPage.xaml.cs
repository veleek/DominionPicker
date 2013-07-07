using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ben.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using GalaSoft.MvvmLight.Threading;

namespace Ben.Dominion
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ApplicationBarIconButton UnlockButton;
        private bool reviewRequestShown = false;
        private bool updatePopupShown = false;
        private bool isNew = false;
        private object favoriteEdit = null;

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
                IconUri = new Uri(@"\Images\appbar.reset.png", UriKind.Relative),
                Text = "reset",
            };
            ResetButton.Click += Reset_Click;

            AddFavoriteButton = new ApplicationBarIconButton
            {
                IconUri = new Uri(@"\Images\appbar.favs.addto.png", UriKind.Relative),
                Text = "save fav...",
            };
            AddFavoriteButton.Click += AddFavorite_Click;
        }

        public MainViewModel MainView
        {
            get { return MainViewModel.Instance; }
        }

        public void LoadState()
        {
            this.MainView.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsGenerating")
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            SystemTray.ProgressIndicator.IsVisible = this.MainView.IsGenerating;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    });
                }
            };

            this.DataContext = this.MainView;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherHelper.Initialize();

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

            if ((App.Current as App).IsNewVersion && appLaunchCount > 1 && !updatePopupShown)
            {
                MessageBox.Show("Some stuff was changed around in this last update as some initial work to allow you to backup and restore your settings and favorites.  But right now your old favorites aren't automatically updated so they'll be missing for the time being.  They're still there, you just can't see them.\nI'll try to get them back in the next update soon.",  "Sorry!", MessageBoxButton.OK);
                updatePopupShown = true;
            }
            else if (appLaunchCount == 10 && !reviewRequestShown)
            {
                RequestReviewPopup.Visibility = Visibility.Visible;
                // This prevents us from showing the popup if they 
                // come back here without exiting the app.
                reviewRequestShown = true;
            }

            this.NavigationService.Initialize();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            //AdManager.UnloadAd(AdContainer);
        }

        private void MainPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            MainViewModel.Instance.CancelGeneration();
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
            if (this.MainView.IsGenerating)
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
                    if (this.MainView.GenerateCardList())
                    {
                        // Navigation has to happen on the UI thread, so ask the Dispatcher to do it
                        Dispatcher.BeginInvoke(() =>
                        {
                            this.NavigationService.Navigate("/ResultsViewer.xaml");
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
            this.MainView.CancelGeneration();

            // Hide the progress bar and swap the button
            //SystemTray.ProgressIndicator.IsVisible = false;
            CreateButton.Content = "Generate";
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            MainView.Reset();
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
                // Make a copy of the settings, so we don't overwrite them
                var newSettings = (e.AddedItems[0] as FavoriteSetting).Value;
                newSettings = GenericXmlSerializer.Deserialize<SettingsViewModel>(GenericXmlSerializer.Serialize(newSettings));

                // HACK: Currently the filtered cards is part of the settings instead of it's own
                // property and/or view model thing, so we have to 'save' it when we load the 
                // settings from the list otherwise we risk losing any cards they've filtered
                // when they use a built in favorite settings.
                newSettings.FilteredCards = MainView.Settings.FilteredCards;

                MainView.Settings = newSettings;
                FavoriteSettingsListBox.SelectedItem = null;

                Create();
            }
        }

        private void FavoriteSetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                // Just in case;
                this.MainView.CancelGeneration();

                this.MainView.Result = (e.AddedItems[0] as FavoriteSet).Value;
                this.NavigationService.Navigate("/ResultsViewer.xaml");

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
            MainViewModel.Instance.SaveFavoriteSettings(name);
        }

        private void ClearFavorites_Click(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("This will delete all your favorites and reset all the settings. \nAre you sure you want to continue?", "Warning", MessageBoxButton.OKCancel);

            if (res == MessageBoxResult.OK)
            {
                // Load up the defaults and clear out what we have 
                MainViewModel.Instance.Favorites = MainViewModel.LoadDefault().Favorites;
                FavoritesScrollViewer.ScrollToVerticalOffset(0);
            }
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
                LoadState();

                RootPivot.SelectedIndex = MainView.PivotIndex;
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
                // Grab the settings object
                var newSettings = favSetting.Value;

                // HACK: Currently the filtered cards is part of the settings instead of it's own
                // property and/or view model thing, so we have to 'save' it when we load the 
                // settings from the list otherwise we risk losing any cards they've filtered
                // when they use a built in favorite settings.
                newSettings.FilteredCards = MainView.Settings.FilteredCards;

                MainView.Settings = newSettings;

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
                MainView.Favorites.FavoriteSets.Remove(favSet);
            }
            else
            {
                FavoriteSetting favSetting = fe.DataContext as FavoriteSetting;
                if (favSetting != null)
                {
                    MainView.Favorites.FavoriteSettings.Remove(favSetting);
                }
            }
        }

        private void RootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MainView.PivotIndex = RootPivot.SelectedIndex;
            Boolean onMainPivot = RootPivot.SelectedIndex == 0;

            if (onMainPivot)
            {
                this.ApplicationBar.Buttons.Add(ResetButton);
                this.ApplicationBar.Buttons.Add(AddFavoriteButton);
                this.ApplicationBar.Mode = ApplicationBarMode.Default;
            }
            else
            {
                this.ApplicationBar.Buttons.Clear();
            }
        }

        void FilterCards_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate("/CardFilterPage.xaml");
        }

        private void About_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate("/AboutPage.xaml");
        }

        private void CardLookup_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate("/TestPage.xaml");
        }
    }
}