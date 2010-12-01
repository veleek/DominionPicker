using System;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Controls;

namespace Ben.Dominion
{
    public partial class MainPage : PhoneApplicationPage
    {
        public PickerState CurrentState;
        public Picker Picker;

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
                this.CurrentState.CurrentPicker = new Picker();
                this.Picker = this.CurrentState.CurrentPicker;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Picker.CreateCardList();
            PickerState.Current.CardList = new System.Collections.ObjectModel.ObservableCollection<Card>();
            foreach(Card c in Picker.CardSet)
            {
                PickerState.Current.CardList.Add(c);
            }
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

        private void SetCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            
            if (checkBox.IsChecked.HasValue)
            {
                CardSet set = (CardSet)checkBox.DataContext;
                CurrentState.CurrentSettings.Sets[set] = checkBox.IsChecked.Value;
                //CurrentState.CurrentSettings.
            }
        }
    }
}