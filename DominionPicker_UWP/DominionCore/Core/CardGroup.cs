using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ben.Data;

namespace Ben.Dominion
{
    public enum CardGroupType
    {
        None,
        KingdomCard,
        BaneRequired,
        OtherRequired,
        PeasantUpgrade,
        PageUpgrade,
        SelectedProsperity,
        SelectedDarkAges,
        Events,
    }

    public class CardGroup
       : ILocalizable
    {
        public CardGroupType Type { get; set; }

        public IEnumerable<Card> Cards { get; set; }


        public CardGroup()
        : this(CardGroupType.None, (Card[])null)
        {
        }

        public CardGroup(CardGroupType type)
        : this(type, (Card[])null)
        {
        }

        public CardGroup(CardGroupType type, Card card)
        : this(type, new[] { card })
        {
        }

        public CardGroup(CardGroupType type, IEnumerable<Card> cards)
        {
            this.Type = type;
            this.Cards = cards;
        }

        public override string ToString()
        {
            return this.ToString(Localized.Strings);
        }

        public string ToString(Localizer localizer)
        {
            return localizer.GetLocalizedValue(Type);
        }

        object[] ILocalizable.LocalizedContext
        {
            get
            {
                if (this.Cards == null)
                {
                    return null;
                }
                return new object[] { string.Join(", ", Cards.Select(c => c.DisplayName)) };
            }
        }
    }

    public class CardGrouping<T>
       : ObservableGrouping<T, Card>
    {
        public CardGrouping(T key, IEnumerable<Card> cards)
        : base(key, cards)
        {
        }

        public void Sort(ResultSortOrder sortOrder)
        {
            IEnumerable<Card> orderedCards;
            if (sortOrder == ResultSortOrder.Name)
            {
                orderedCards = this.OrderBy(c => c.Name);
            }
            else
            {
                Type cardType = typeof(Card);
                var sortProperty = cardType.GetProperty(sortOrder.ToString(), BindingFlags.Public | BindingFlags.Instance);
                orderedCards = this.OrderBy(c => sortProperty.GetValue(c)).ThenBy(c => c.Name);
            }
            List<Card> newCardsList = orderedCards.ToList();
            this.Clear();
            foreach (Card card in newCardsList)
            {
                this.Add(card);
            }
        }
    }
}