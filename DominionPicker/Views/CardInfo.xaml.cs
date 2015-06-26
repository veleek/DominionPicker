using System.Windows;
using Microsoft.Phone.Controls;

namespace Ben.Dominion
{
    public partial class CardInfo : PhoneApplicationPage
    {
        public CardInfo()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(CardInfo_Loaded);
        }

        void CardInfo_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = App.Instance.SelectedCard;
        }

        private void CardInfo_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                int currentIndex = Cards.AllCards.IndexOf(this.DataContext as Card);
                int nextIndex = e.HorizontalVelocity < 0 ? currentIndex + 1 : currentIndex - 1;

                if (nextIndex < 0 || nextIndex >= Cards.AllCards.Count)
                {
                    return;
                }

                var nextCard = Cards.AllCards[nextIndex];
                this.DataContext = nextCard;
            }
        }
    }
}