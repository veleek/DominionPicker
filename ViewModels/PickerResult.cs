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

        private CardList pool = null;
        private CardList cards = null;
        private CardList additionalCards = null;
        private List<string> additionalStuff = null;
        private ResultSortOrder sortOrder;

        public PickerResult()
        {
        }

        public CardList Pool
        {
            get { return pool; }
            set { this.SetProperty(ref pool, value, "Pool"); }
        }

        public CardList Cards
        {
            get { return cards; }
            set { this.SetProperty(ref cards, value, "Cards"); }
        }

        public CardList AdditionalCards
        {
            get { return additionalCards; }
            set { this.SetProperty(ref additionalCards, value, "AdditionalCards"); }
        }

        public List<String> AdditionalStuff
        {
            get { return additionalStuff; }
            set { this.SetProperty(ref additionalStuff, value, "AdditionalStuff"); }
        }

        public ResultSortOrder SortOrder
        {
            get { return sortOrder; }
            set { this.SetProperty(ref sortOrder, value, "SortOrder"); }
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

        public Boolean HasExtras { get { return (this.AdditionalCards != null && this.AdditionalCards.Count > 0) || (this.AdditionalStuff != null && this.AdditionalStuff.Count > 0); } }

        public void Replace(Card target)
        {
            if (target == null)
            {
                return;
            }

            var replacement = Pool.GetRandomCardExcept(Cards);

            if (replacement == null)
            {
                return;
            }

            Replace(target, replacement);
        }

        public void Replace(Card target, Card replacement)
        {
            if (!this.Cards.Contains(target))
            {
                // This occurse if you swipe an 'additional card'
                return;
                //throw new InvalidOperationException("You can't replace a card that's not in the result");
            }

            int index = -1;
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].Name == target.Name)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                System.Diagnostics.Debugger.Break();

            Pool.Remove(replacement);
            Pool.Add(target);

            Cards[index] = replacement;
            
        }

        public void Sort(ResultSortOrder newSortOrder)
        {
            if (Cards == null)
            {
                return;
            }

            this.sortOrder = newSortOrder;
            switch (this.sortOrder)
            {
                case ResultSortOrder.Cost:
                    Cards = Cards.OrderBy(c => c.Cost).ToCardList();
                    break;
                case ResultSortOrder.Set:
                    // Order them by set then alphabetically
                    // This makes the most sense because it's probably 
                    // being used by someone who has the cards in the 
                    // original set boxes
                    Cards = Cards.OrderBy(c => c.Set + "," + c.DisplayName).ToCardList();
                    break;
                case ResultSortOrder.Name:
                default:
                    Cards = Cards.OrderBy(c => c.DisplayName).ToCardList();
                    // Orders them randomly, not used
                    ////Cards = Cards.OrderBy(c => Guid.NewGuid()).ToCardList();
                    break;
            }
        }

        public override string ToString()
        {
            if (Cards == null)
            {
                return "Cards is Null";
            }

            return Cards.Select(c => c.Set).Distinct().Select(s => s.ToString().Substring(0, 4)).Aggregate((a, b) => a + ", " + b);
        }

        public List<int> ToList()
        {
            return Cards == null ? new List<int>() : Cards.Select(c => c.ID).ToList();
        }

        public static PickerResult FromList(IEnumerable<Int32> cardIds)
        {
            PickerResult result = new PickerResult();
            result.Cards = cardIds.Select(id => Card.FromId(id)).ToCardList();
            return result;
        }
    }
}
