using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Ben.Dominion.Controls;
//using GoogleAnalytics;
using Ben.Dominion.Resources;
using Ben.Dominion.Utilities;
using Ben.Dominion.ViewModels;
using Ben.Dominion.Views;
using Ben.Utilities;
using GalaSoft.MvvmLight.Threading;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Ben.Dominion
{

    public partial class MainPage : Page
    {
        private object favoriteEdit;
        private bool reviewRequestShown;
        private bool updatePopupShown;

        public MainPage()
        {
            // Force the resources to be initialized before anything tries to access them.
            var x = Strings.About_About;
            var y = CardDataStrings.Application_LocalizedCardsFileName;
            PickerViews.Initialize();

            this.InitializeComponent();

            DispatcherHelper.Initialize();

            // Set the data context of the listbox control to the sample data
            this.Loaded += this.MainPage_Loaded;

            if (Debugger.IsAttached)
            {
                (BottomAppBar as CommandBar).AddMenuItem("Debug", (s, a) => NavigationServiceHelper.Navigate(PickerView.Debug));
            }
        }

        public MainViewModel MainView
        {
            get
            {
                return MainViewModel.Instance;
            }
        }

        public void LoadState()
        {
            this.MainView.PropertyChanged += (s, e) =>
               {
                   if (e.PropertyName == "IsGenerating")
                   {
                       IAsyncAction showProgressBarTask = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                       {
                           try
                           {
                               //WindowsPhoneUWP.UpgradeHelpers.ProgressIndicator.ChangeVisibility(Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ProgressIndicator, this.MainView.IsGenerating);
                           }
                           catch (Exception ex)
                           {
                               AppLog.Instance.Error("An error occurred while attempting to show the progress indicator", ex);
                           }
                       });
                   }
               };
            this.DataContext = this.MainView;
        }

        public async Task ShowAddFavoritePopupAsync()
        {
            this.favoriteEdit = null;
            ContentDialog addFavoriteDialog = new AddFavoriteDialog();
            var result = await addFavoriteDialog.ShowAsync();
        }

        private void Create()
        {
            if (this.MainView.IsGenerating)
            {
                this.StopGeneration();
            }
            else
            {
                this.StartGeneration();
            }
        }

        private void StartGeneration()
        {
            // Start and show the progress bar, and change the create button
            //SystemTray.ProgressIndicator.IsVisible = true;
            this.CreateButton.Content = "Cancel";
            // We don't want the generation to happen on the UI thread.
            // A background worker will enable stuff to continue (e.g.
            // quit the app) while the generation is happening.
            var generateWorker = new BackgroundWorker();
            generateWorker.DoWork += async (backgroundSender, backgroundArgs) =>
            {
                try
                {
                    if (this.MainView.GenerateCardList())
                    {
                        // Navigation has to happen on the UI thread, so ask the Dispatcher to do it
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PickerView.Results.Go());
                    }
                    else
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            async () =>
                            {
                                MessageDialog dialog = new MessageDialog("Whoops! We couldn't generate a set with those options.")
                                {
                                    Commands =
                                    {
                                        new UICommand() {Id = 10, Label = "OK" },
                                    }
                                };

                                await dialog.ShowAsync();
                            }
                );
                    }
                }
                finally
                {
                    // And finally when everything is done, ask the UI thread to reenable
                    // the buttons and hide the progress bar
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.StopGeneration);
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
            this.CreateButton.Content = "Generate";
        }

        private void SaveFavoriteSettings(String name)
        {
            MainViewModel.Instance.SaveFavoriteSettings(name);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                // Load the state
                this.LoadState();
                this.RootPivot.SelectedIndex = this.MainView.PivotIndex;
                // We want to track that MainPage was navigated to and the NavigationService
                // has not been setup to handle that yet, so we'll just manually send the event
                // here.
                //EasyTracker.GetTracker().SendView(new Uri("/" + e.SourcePageType.Name + ".xaml", UriKind.Relative).ToString());
                // Handle any voice commands
            }

            base.OnNavigatedTo(e);
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //AdManager.LoadAd(AdContainer);
            var appLaunchCount = 0;
            Windows.Storage.ApplicationData.Current.LocalSettings.TryGetValue("AppLaunchCount", out appLaunchCount);
            // If this is a new app version and we haven't done any one-time upgrade stuff
            if (App.Instance.IsNewVersion && appLaunchCount > 1 && !this.updatePopupShown)
            {
                this.updatePopupShown = true;
                UpdateDialog updateDialog = new UpdateDialog();
                await updateDialog.ShowAsync();
            }

            if (appLaunchCount == 10 && !this.reviewRequestShown)
            {
                // This prevents us from showing the popup if they 
                // come back here without exiting the app.
                this.reviewRequestShown = true;

                RequestReviewDialog reviewDialog = new RequestReviewDialog();
                await reviewDialog.ShowAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (MainViewModel.Instance.IsGenerating) MainViewModel.Instance.CancelGeneration();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            this.Create();
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            this.MainView.Reset();
            this.SettingsScrollViewer.ChangeView(null, 0, null, true);
            this.SettingsScrollViewer.UpdateLayout();
        }

        private async void AddFavorite_Click(object sender, RoutedEventArgs e)
        {
            await this.ShowAddFavoritePopupAsync();
        }

        private async void ResetFavorites_Click(object sender, RoutedEventArgs e)
        {

            var deleteFavoritesDialog = new MessageDialog("This will delete all your favorites and reset all the settings. \nAre you sure you want to continue?", "Warning")
            {
                Commands =
                {
                    new UICommand { Id = 10, Label = "OK" },
                    new UICommand { Id = 20, Label = "Cancel" }
                }
            };

            var label = (await deleteFavoritesDialog.ShowAsync()).Label;
            if (label == "OK")
            {
                // Load up the defaults and clear out what we have 
                MainViewModel.Instance.Favorites = (await MainViewModel.LoadDefault()).Favorites;
                this.FavoritesScrollViewer.ChangeView(null, 0, null, true);
                this.FavoritesScrollViewer.UpdateLayout();
            }
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
                newSettings.FilteredCards = this.MainView.Settings.FilteredCards;
                this.MainView.Settings = newSettings;
                this.FavoriteSettingsListBox.SelectedItem = null;
                this.Create();
            }
        }

        private void FavoriteSetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                // Just in case;
                this.MainView.CancelGeneration();
                this.MainView.Result = (e.AddedItems[0] as FavoriteSet).Value;
                NavigationServiceHelper.Navigate(PickerView.Results);
                // Clear the selection
                this.FavoriteSetsListBox.SelectedItem = null;
            }
        }

        private void AddFavoritePopup_SaveFavorite(object sender, FavoriteEventArgs e)
        {
            if (this.favoriteEdit == null)
            {
                this.SaveFavoriteSettings(e.FavoriteName);
                this.RootPivot.SelectedIndex = 1;
            }
            else if (this.favoriteEdit is FavoriteSet)
            {
                (this.favoriteEdit as FavoriteSet).Name = e.FavoriteName;
            }
            else if (this.favoriteEdit is FavoriteSetting)
            {
                (this.favoriteEdit as FavoriteSetting).Name = e.FavoriteName;
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var favSet = fe.DataContext as FavoriteSet;
            if (favSet != null)
            {
                //this.AddFavoritePopup.FavoriteName = favSet.Name;
                this.favoriteEdit = favSet;
            }
            else
            {
                var favSetting = fe.DataContext as FavoriteSetting;
                if (favSetting != null)
                {
                    //this.AddFavoritePopup.FavoriteName = favSetting.Name;
                    this.favoriteEdit = favSetting;
                }
            }
            // Allow this event to complete, and then open the popup after
            // that so that we don't steal the focus away from the textbox.
            // *sigh* Yes, it could have been better to just use a separate 
            // page for the name dialog.
            var showFavoriteTask = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await this.ShowAddFavoritePopupAsync();
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var favSetting = fe.DataContext as FavoriteSetting;
            if (favSetting != null)
            {
                // Grab the settings object
                var newSettings = favSetting.Value;
                // HACK: Currently the filtered cards is part of the settings instead of it's own
                // property and/or view model thing, so we have to 'save' it when we load the 
                // settings from the list otherwise we risk losing any cards they've filtered
                // when they use a built in favorite settings.
                newSettings.FilteredCards = this.MainView.Settings.FilteredCards;
                this.MainView.Settings = newSettings;
                // Navigate back to the settings pivot and scroll to the top
                this.RootPivot.SelectedIndex = 0;
                this.SettingsScrollViewer.ChangeView(null, 0, null, true);
                this.SettingsScrollViewer.UpdateLayout();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var favSet = fe.DataContext as FavoriteSet;
            if (favSet != null)
            {
                this.MainView.Favorites.FavoriteSets.Remove(favSet);
            }
            else
            {
                var favSetting = fe.DataContext as FavoriteSetting;
                if (favSetting != null)
                {
                    this.MainView.Favorites.FavoriteSettings.Remove(favSetting);
                }
            }
        }
    }
}