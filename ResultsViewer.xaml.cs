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
        public IList<Card> Cards { get { return PickerState.Current.CardList; } }

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

                PickerState.Current.ReplaceCard(element.DataContext as Card);
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            PickerState.Current.GenerateCardList();
        }

        private void Sort_Click(object sender, EventArgs e)
        {

        }
    }
}
