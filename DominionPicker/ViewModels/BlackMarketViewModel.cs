using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class BlackMarketViewModel : NotifyPropertyChangedBase
    {
        private static ReadOnlyCollection<Card> baseBlackMarketCards;

        private CardList deck;
        private CardList hand;
        private Card selected;

        public BlackMarketViewModel()
        {
            if (DesignHelper.IsInDesignMode)
            {
                this.Deck = new CardList();
                this.Selected = new Card("Witch", CardType.Action | CardType.Attack) { Set = CardSet.Base };
            }
        }

        public static ReadOnlyCollection<Card> BaseBlackMarketCards
        {
            get
            {
                if (baseBlackMarketCards == null)
                {
                    baseBlackMarketCards = new ReadOnlyCollection<Card>(
                        Cards.PickableCards.Except(new[] { Card.FromName("Black Market") }).ToList()
                    );
                }

                return baseBlackMarketCards;
            }
        }

        public CardList Deck
        {
            get { return this.deck; }
            set { this.SetProperty(ref this.deck, value, "Deck"); }
        }

        public CardList Hand
        {
            get { return this.hand; }
            set { this.SetProperty(ref this.hand, value, "Hand"); }
        }

        public Card Selected
        {
            get { return this.selected; }
            set { this.SetProperty(ref this.selected, value, "Selected"); }
        }

        public void Reset()
        {
            this.Reset(new CardList());
        }

        public void Reset(CardList supplyCards)
        {
            if (supplyCards == null)
            {
                throw new ArgumentNullException("supplyCards");
            }

            this.Deck = BaseBlackMarketCards.Except(supplyCards).OrderBy(c => Guid.NewGuid()).ToCardList();
            this.Hand = new CardList();
            this.Selected = null;
        }

        public void Draw()
        {
            this.Hand = this.Deck.Draw(3);
        }

        public void Discard()
        {
            this.Selected = null;

            foreach (var card in this.Hand)
            {
                this.Deck.Add(card);
            }

            this.Hand.Clear();
            // TODO: This may be needed, but it's an observable collection so maybe not.
            //this.OnPropertyChanged("Hand");
        }

        public void Pick(Card c)
        {
            if (c == this.Selected)
            {
                this.Hand.Add(this.Selected);
                this.Selected = null;
                return;
            }

            if (!this.Hand.Contains(c))
            {
                throw new ArgumentException("The card being picked must be in the hand");
            }

            if (this.Selected != null)
            {
                this.Hand.Add(this.Selected);
            }

            this.Hand.Remove(c);
            this.Selected = c;
        }

        public void Replace(Card c)
        {
            int index = this.Hand.IndexOf(c);
            if (index >= 0 && index < this.Hand.Count)
            {
                this.Hand[this.Hand.IndexOf(c)] = this.Deck.Draw();
            }
        }
    }
}
