using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Ben.Utilities;
using System.Collections.Generic;

namespace Ben.Dominion
{
    public class PickerResult : NotifyPropertyChangedBase
    {
        private static Regex actionRegex = new Regex(@"\+[1-9] Action");
        private static Regex twoActionRegex = new Regex(@"\+[2-9] Actions");
        private static Regex buyRegex = new Regex(@"\+[1-9] Buy");
        private static Regex twoBuyRegex = new Regex(@"\+[2-9] Buys");
        private static Regex trashRegex = new Regex(@"Trash(?! this)");

        public PickerResult()
        {
            AdditionalCards = new ObservableCollection<Card>();
        }

        private ObservableCollection<Card> cards = null;
        public ObservableCollection<Card> Cards
        {
            get { return cards; }
            set
            {
                if (cards != value)
                {
                    cards = value;
                    NotifyPropertyChanged("Cards");
                }
            }
        }

        private ObservableCollection<Card> additionalCards = null;
        public ObservableCollection<Card> AdditionalCards
        {
            get { return additionalCards; }
            set
            {
                if (additionalCards != value)
                {
                    additionalCards = value;
                    NotifyPropertyChanged("AdditionalCards");
                }
            }
        }

        public Boolean HasCardType(CardType type)
        {
            return Cards.Any(c => c.IsType(type));
        }

        public Boolean HasCard(Card card)
        {
            return Cards.Contains(card);
        }

        public Boolean HasCard(String name)
        {
            return HasCard(Card.FromName(name));
        }

        public Boolean HasAttack { get { return HasCardType(CardType.Attack); } }
        public Boolean HasReaction { get { return HasCardType(CardType.Reaction); } }

        public Boolean HasTrash { get { return Cards.Any(c => trashRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlusAction { get { return Cards.Any(c => actionRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlus2Action { get { return Cards.Any(c => twoActionRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlusBuy { get { return Cards.Any(c => buyRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlus2Buy { get { return Cards.Any(c => twoBuyRegex.IsMatch(c.Rules)); } }

        public override string ToString()
        {
            if (Cards == null)
            {
                return "Cards is Null";
            }

            return Cards.Select(c => c.Set).Distinct().Select(s => s.ToString().Substring(0, 4)).Aggregate((a, b) => a + ", " + b);
        }

        public List<Int32> ToList()
        {
            return Cards.Select(c => c.ID).ToList();
        }

        public static PickerResult FromList(IEnumerable<Int32> cardIds)
        {
            PickerResult result = new PickerResult();
            result.Cards = cardIds.Select(id => Card.FromId(id)).ToObservableCollection();
            return result;
        }
    }
}
