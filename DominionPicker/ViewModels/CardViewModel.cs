using Ben.Dominion.Resources;
using Ben.Utilities;
using System;
using System.Linq;

namespace Ben.Dominion
{
    public class CardViewModel : NotifyPropertyChangedBase
    {
        private Card card;

        public CardViewModel()
        {
        }

        public CardViewModel(Card card)
        {
            this.Card = card;
        }

        public Card Card
        {
            get { return card; }
            set { this.SetProperty(ref card, value, "Card"); }
        }

        public static implicit operator CardViewModel(Card card)
        {
            return new CardViewModel(card);
        }
    }
}
