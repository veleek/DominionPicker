﻿using System;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using Ben.Utilities;

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

        public void LoadState()
        {
            if (this.CurrentState == null)
            {
                // This should already be loaded by this time
                this.CurrentState = PickerState.Current;
                this.DataContext = PickerState.Current;

                AppLog.Instance.Log("Test Line");
                AppLogListBox.ItemsSource = AppLog.Instance.Lines;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadState();
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

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState.LoadSettings(((FrameworkElement)sender).DataContext as PickerSettings);
            RootPivot.SelectedIndex = 0;
        }

        private void ClearState_Click(object sender, EventArgs e)
        {
            PickerState.ResetState();
            LoadState();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            PickerState.Save();
        }

        private void AddFavorite_Click(object sender, EventArgs e)
        {
            AddFavoritePopup.IsOpen = !AddFavoritePopup.IsOpen;
        }
    }
}