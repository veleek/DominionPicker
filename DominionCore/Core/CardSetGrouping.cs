using Ben.Data;
using System.Collections.Generic;

namespace Ben.Dominion
{
    /// <summary>
    /// A grouping of card selectors by set
    /// </summary>
    public class CardSetGrouping
       : ObservableGrouping<CardSet, CardSelector>
    {
        public CardSetGrouping(CardSet key, IEnumerable<CardSelector> cards)
        : base(key, cards)
        {
        }

        public void SortedInsert(CardSelector card)
        {
            int i;
            for (i = 0; i < this.Count; i++)
            {
                if (card.CompareTo(this[i]) < 0)
                {
                    break;
                }
            }
            this.Insert(i, card);
        }

        public override string ToString()
        {
            return "CardSetGrouping: " + this.Key;
        }
    }
}
