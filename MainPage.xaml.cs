using System;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using Ben.Utilities;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Ben.Dominion
{
    public partial class MainPage : PhoneApplicationPage
    {
        public PickerState CurrentState;
        public ApplicationBarIconButton unlockButton;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.Unloaded += new RoutedEventHandler(MainPage_Unloaded);

            unlockButton = new ApplicationBarIconButton(new Uri("/Images/appbar.lock.png", UriKind.Relative));
            unlockButton.Click += new EventHandler(unlockButton_Click);
            unlockButton.Text = "Lock";
            
            LoadState();
        }

        public void LoadState()
        {
            if (this.CurrentState == null)
            {
                this.CurrentState = PickerState.Current;
                this.DataContext = PickerState.Current;
                //AppLogListBox.ItemsSource = AppLog.Instance.Lines;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            App app = App.Current as App;

            app.InitializeAdControl(AdContainer);

            if (app.IsTrial)
            {
                this.ApplicationBar.Buttons.Add(unlockButton);    
            }
            else
            {
                if (this.ApplicationBar.Buttons.Contains(unlockButton))
                {
                    this.ApplicationBar.Buttons.Remove(unlockButton);
                }
            }
        }

        void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App app = App.Current as App;
            app.UnitializeAdControl(AdContainer);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentState.GenerateCardList();
            this.NavigationService.Navigate(new Uri("/ResultsViewer.xaml", UriKind.Relative));
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

        void unlockButton_Click(object sender, EventArgs e)
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
                PickerState.ResetState();
                LoadState();
                CurrentState.Reset();
                RootPivot.SelectedIndex = 0;
                SettingsScrollViewer.ScrollToVerticalOffset(0);
                FavoritesListBox.ItemsSource = PickerState.Current.FavoriteSettings;
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
			this.NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }
    }
}