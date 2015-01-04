using Ben.Dominion.Resources;
using Ben.Utilities;
using System;
using System.Linq;

namespace Ben.Dominion
{
    public class CardViewModel : NotifyPropertyChangedBase
    {
        private Card card;
        private CardSetViewModel setView;
        private CardTypeViewModel typeView;

        public CardViewModel()
        {
        }

        public CardViewModel(Card card)
        {
            this.Card = card;
        }

        public CardSetViewModel SetView
        {
            get { return setView; }
        }

        public CardTypeViewModel TypeView
        {
            get { return typeView; }
        }

        public Card Card
        {
            get { return card; }
            set
            {
                if (this.SetProperty(ref card, value, "Card"))
                {
                    setView = card.Set;
                    typeView = card.Type;
                }
            }
        }

        public static implicit operator CardViewModel(Card card)
        {
            return new CardViewModel(card);
        }
    }
}
