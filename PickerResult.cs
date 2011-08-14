using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class PickerResult : NotifyPropertyChangedBase
    {
        private static Regex actionRegex = new Regex(@"\+[1-9] Action");
        private static Regex twoActionRegex = new Regex(@"\+[2-9] Actions");
        private static Regex buyRegex = new Regex(@"\+[1-9] Buy");
        private static Regex twoBuyRegex = new Regex(@"\+[2-9] Buys");
        private static Regex trashRegex = new Regex(@"Trash(?! this)");

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
        public Boolean RequireCurses { get; set; }
        public Boolean RequireProvinceAndPlatinum { get; set; }
        public Boolean RequirePrizes { get; set; }

        public Boolean HasCardType(CardType type)
        {
            return Cards.Any(c => c.IsType(type));
        }

        public Boolean HasAttack { get { return HasCardType(CardType.Attack); } }
        public Boolean HasReaction { get { return HasCardType(CardType.Reaction); } }

        public Boolean HasTrash { get { return Cards.Any(c => trashRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlusAction { get { return Cards.Any(c => actionRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlus2Action { get { return Cards.Any(c => twoActionRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlusBuy { get { return Cards.Any(c => buyRegex.IsMatch(c.Rules)); } }
        public Boolean HasPlus2Buy { get { return Cards.Any(c => twoBuyRegex.IsMatch(c.Rules)); } }
    }
}
