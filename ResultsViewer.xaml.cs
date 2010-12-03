using System.Collections.Generic;
using Microsoft.Phone.Controls;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.Phone.Shell;

namespace Ben.Dominion
{
    public partial class ResultsViewer : PhoneApplicationPage
    {
        public ResultsViewer()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ResultsViewer_Loaded);
        }

        void ResultsViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = PickerState.Current;
        }

        private void CardItem_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                FrameworkElement element = sender as FrameworkElement;
                if (element == null)
                {
                    return;
                }

                PickerState.Current.ReplaceCard(element.DataContext as Card);
            }
        }

        private void CardItem_Tap(object sender, GestureEventArgs e)
        {
            (App.Current as App).SelectedCard = sender.GetContext<Card>();
            NavigationService.Navigate(new Uri("/CardInfo.xaml", UriKind.Relative));
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            PickerState.Current.GenerateCardList();
        }

        private void Sort_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;
            PickerState.Current.SwapSort();
        }
    }
}
