using System;

namespace Ben.Dominion
{
    /// <summary>
    /// A selector for managing selecting and filtering cards in the Card Info list.
    /// </summary>
    public class CardSelector : IComparable<CardSelector>
    {
        public CardSelector()
        {
        }

        public CardSelector(Card card, bool selected)
        {
            this.Card = card;
            this.Selected = selected;
        }

        public Card Card { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether this card is checked or
        /// not checked in the Card Info list which identifies whether it will
        /// appear in results or not.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether this card has been filtered
        /// out of the visible set of cards using the search box.
        /// </summary>
        public bool Filtered { get; set; }

        public int CompareTo(CardSelector other)
        {
            return String.CompareOrdinal(this.Card.Name, other.Card.Name);
        }

        public bool Filter(bool filtered)
        {
            if (filtered == this.Filtered)
            {
                return false;
            }
            this.Filtered = filtered;
            return true;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"[{(this.Selected ? "X" : "_")}] {this.Card}";
        }
    }
}
