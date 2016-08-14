using System;
using System.ComponentModel;
using System.Diagnostics;
//using GoogleAnalytics;
using Ben.Dominion.Models;
using Ben.Dominion.Resources;
using Ben.Dominion.TestControls;
using Ben.Dominion.Utilities;
using Ben.Dominion.Views;
using Ben.Utilities;
using GalaSoft.MvvmLight.Threading;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Ben.Dominion
{

    public partial class MainPage : Page
    {
        private object favoriteEdit;
        private bool reviewRequestShown;
        private bool updatePopupShown;
        private readonly AppBarButton addFavoriteButton;
        private readonly AppBarButton resetFavoritesButton;
        private readonly AppBarButton resetSettingsButton;

        public MainPage()
        {
            // Force the resources to be initialized before anything tries to access them.
            var x = Ben.Dominion.Resources.Strings.About_About;
            var y = Ben.Dominion.Resources.CardDataStrings.Application_LocalizedCardsFileName;

            this.InitializeComponent();

            DispatcherHelper.Initialize();
            var backgroundBrush = this.RequestReviewPopup.Background as SolidColorBrush;
            var backgroundColor = backgroundBrush.Color;
            backgroundColor.A = 0x66;
            backgroundBrush.Color = backgroundColor;
            
            // Set the data context of the listbox control to the sample data
            this.Loaded += this.MainPage_Loaded;
            this.Unloaded += this.MainPage_Unloaded;

            SystemNavigationManager.GetForCurrentView().BackRequested += this.MainPage_BackKeyPress;
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_CardLookup, this.CardLookup_Click);
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_BlackMarket, this.BlackMarket_Click);
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_Settings, this.Settings_Click);
            ((CommandBar)BottomAppBar).AddMenuItem(Strings.Menu_About, this.About_Click);
            if (Debugger.IsAttached)
            {
                ((CommandBar)BottomAppBar).AddMenuItem("Debug", (s, a) => NavigationServiceHelper.Navigate(PickerView.Debug));
            }
            // Create all the app bar buttons as well.
            this.resetSettingsButton = ApplicationBarHelper.CreateIconButton(Strings.MainPage_Reset, @"\Images\appbar.reset.png", this.ResetSettings_Click);
            this.addFavoriteButton = ApplicationBarHelper.CreateIconButton(Strings.MainPage_Save, @"\Images\appbar.favs.addto.png", this.AddFavorite_Click);
            this.resetFavoritesButton = ApplicationBarHelper.CreateIconButton(Strings.MainPage_Reset, @"\Images\appbar.reset.png", this.ResetFavorites_Click);
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

        public void ShowAddFavoritePopup()
        {
            this.AddFavoritePopup.IsOpen = true;
            this.resetSettingsButton.IsEnabled = false;
            this.addFavoriteButton.IsEnabled = false;
        }

        public void HideAddFavoritePopup()
        {
            this.AddFavoritePopup.IsOpen = false;
            this.resetSettingsButton.IsEnabled = true;
            this.addFavoriteButton.IsEnabled = true;
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

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
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

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //AdManager.LoadAd(AdContainer);
            var appLaunchCount = 0;
            Windows.Storage.ApplicationData.Current.LocalSettings.TryGetValue("AppLaunchCount", out appLaunchCount);
            // If this is a new app version and we haven't done any one-time upgrade stuff
            if (App.Instance.IsNewVersion && appLaunchCount > 1 && !this.updatePopupShown)
            {
                this.UpdatePopup.Visibility = Visibility.Visible;
            }
            if (appLaunchCount == 10 && !this.reviewRequestShown)
            {
                this.RequestReviewPopup.Visibility = Visibility.Visible;
                // This prevents us from showing the popup if they 
                // come back here without exiting the app.
                this.reviewRequestShown = true;
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            //AdManager.UnloadAd(AdContainer);
        }

        private void MainPage_BackKeyPress(object sender, BackRequestedEventArgs e)
        {
            MainViewModel.Instance.CancelGeneration();
            if (this.AddFavoritePopup.IsOpen)
            {
                this.HideAddFavoritePopup();
                e.Handled = true;
            }
        }

        private void RootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MainView.PivotIndex = this.RootPivot.SelectedIndex;
            ((CommandBar)BottomAppBar).PrimaryCommands.Clear();
            switch (this.RootPivot.SelectedIndex)
            {
                case 0:
                    ((CommandBar)BottomAppBar).PrimaryCommands.Add(this.resetSettingsButton);
                    ((CommandBar)BottomAppBar).PrimaryCommands.Add(this.addFavoriteButton);
                    break;
                case 1:
                    ((CommandBar)BottomAppBar).PrimaryCommands.Add(this.resetFavoritesButton);
                    break;
            }
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

        private void AddFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (!this.AddFavoritePopup.IsOpen)
            {
                this.favoriteEdit = null;
                this.AddFavoritePopup.FavoriteName = String.Empty;
                this.ShowAddFavoritePopup();
            }
        }

        private async void ResetFavorites_Click(object sender, RoutedEventArgs e)
        {
            var res = ((int)(await (new Windows.UI.Popups.MessageDialog("This will delete all your favorites and reset all the settings. \nAre you sure you want to continue?", "Warning")
            {
                Commands =
                  {
                     new Windows.UI.Popups.UICommand()
                     {
                        Id = 10, Label = "OK"
                     },
                     new Windows.UI.Popups.UICommand()
                     {
                        Id = 20, Label = "Cancel"
                     }
                  }
            }).ShowAsync()).Id);
            if (res == 10)
            {
                // Load up the defaults and clear out what we have 
                MainViewModel.Instance.Favorites = (await MainViewModel.LoadDefault()).Favorites;
                this.FavoritesScrollViewer.ChangeView(null, 0, null, true);
                this.FavoritesScrollViewer.UpdateLayout();
            }
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

        private async void RequestReviewOk_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + Windows.ApplicationModel.Package.Current.Id.Name));
            this.RequestReviewPopup.Visibility = Visibility.Collapsed;
        }

        private void RequestReviewCancel_Click(object sender, RoutedEventArgs e)
        {
            this.RequestReviewPopup.Visibility = Visibility.Collapsed;
        }

        private void UpdatePopupOk_Click(object sender, RoutedEventArgs e)
        {
            this.UpdatePopup.Visibility = Visibility.Collapsed;
            this.updatePopupShown = true;
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
            this.HideAddFavoritePopup();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var favSet = fe.DataContext as FavoriteSet;
            if (favSet != null)
            {
                this.AddFavoritePopup.FavoriteName = favSet.Name;
                this.favoriteEdit = favSet;
            }
            else
            {
                var favSetting = fe.DataContext as FavoriteSetting;
                if (favSetting != null)
                {
                    this.AddFavoritePopup.FavoriteName = favSetting.Name;
                    this.favoriteEdit = favSetting;
                }
            }
            // Allow this event to complete, and then open the popup after
            // that so that we don't steal the focus away from the textbox.
            // *sigh* Yes, it could have been better to just use a separate 
            // page for the name dialog.
            var showFavoriteTask = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
               {
                   this.ShowAddFavoritePopup();
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