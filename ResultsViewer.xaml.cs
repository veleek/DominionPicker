using System.Collections.Generic;
using Microsoft.Phone.Controls;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ben.Dominion
{
    public partial class ResultsViewer : PhoneApplicationPage
    {
        public List<Card> Cards { get { return PickerState.Current.CurrentPicker.CardSet; } }

        public ResultsViewer()
        {
            InitializeComponent();
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

                Card c = element.DataContext as Card;
                if (c == null)
                {
                    return;
                }

                Int32 index = Cards.IndexOf(c);
                Cards.Remove(c);
                Cards.Insert(index, new Card("Mint", CardType.Action));
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            var c = LayoutRoot.DataContext;
            var b = CardsList.GetBindingExpression(ItemsControl.ItemsSourceProperty);
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
