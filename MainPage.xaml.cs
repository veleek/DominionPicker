using System;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

namespace Ben.Dominion
{
    public partial class MainPage : PhoneApplicationPage
    {
        public PickerState CurrentState;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.CurrentState == null)
            {
                this.CurrentState = PickerState.Current;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentState.GenerateCardList();
            this.NavigationService.Navigate(new Uri("/ResultsViewer.xaml", UriKind.Relative));
        }

        private void AddFavorite_Click(object sender, EventArgs e)
        {
            CurrentState.SaveFavorite();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            CurrentState.Reset();
        }


        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState.LoadSettings(((FrameworkElement)sender).DataContext as PickerSettings);
            RootPivot.SelectedIndex = 0;
        }

        private void ClearState_Click(object sender, EventArgs e)
        {
            PickerState.ResetState();
        }
    }
}