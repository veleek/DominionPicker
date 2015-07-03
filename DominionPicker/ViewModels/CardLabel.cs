namespace Ben.Dominion.ViewModels
{
    using Data;
    
    using System.Collections.Generic;
    using System.Linq;

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
    }

    public class CardGroup : ILocalizable
    {
        public CardGroupType Type { get; set; }

        public IEnumerable<Card> Cards { get; set; }

        public CardGroup() : this(CardGroupType.None, (Card[])null) { }

        public CardGroup(CardGroupType type) : this(type, (Card[])null)
        {

        }

        public CardGroup(CardGroupType type, Card card) : this(type, new[] { card })
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

    public class CardGrouping<T> : ObservableGrouping<T, Card>
    {
        public CardGrouping(T key, IEnumerable<Card> cards)
            : base(key, cards)
        {
        }
    }
}
