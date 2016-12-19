using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Ben.Dominion
{
    public partial class CardInfo : Page
    {
        public CardInfo()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Card selectedCard = e.Parameter as Card;

            if(selectedCard != null)
            {
                this.DataContext = selectedCard;
            }
        }

        //private void CardInfo_Flick(object sender, FlickGestureEventArgs e)
        //{
        //   if ( e.Direction == Windows.UI.Xaml.Controls.Orientation.Horizontal )
        //   {
        //      int currentIndex = Cards.AllCards.IndexOf(this.DataContext as Card);
        //      int nextIndex = e.HorizontalVelocity < 0 ? currentIndex + 1 : currentIndex - 1;
        //      if ( nextIndex < 0 || nextIndex >= Cards.AllCards.Count )
        //      {
        //         return ;
        //      }
        //      var nextCard = Cards.AllCards[nextIndex];
        //      this.DataContext = nextCard;
        //   }
        //}
    }
}