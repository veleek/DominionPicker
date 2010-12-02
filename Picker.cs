using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public class Picker
    {
        private List<Card> cardSet { get; set; }
        private List<Card> cardPool { get; set; }
        
        public ObservableCollection<Card> GenerateCardList()
        {
            return GenerateCardList(PickerState.Current.CurrentSettings);
        }

        public ObservableCollection<Card> GenerateCardList(PickerSettings settings)
        {
            cardPool = Cards.AllCards.Where(c => c.InSet(settings.SelectedSets)).ToList();
            cardSet = cardPool.OrderBy(c => Guid.NewGuid()).Take(10).ToList();

            // Order them alphabetically
            cardSet = cardSet.OrderBy(c => c.Name).ToList();

            ObservableCollection<Card> finalList = new ObservableCollection<Card>();
            foreach (Card c in cardSet)
            {
                finalList.Add(c);
            }

            return finalList;
        }

        public Card GetRandomCard()
        {
            return cardPool.OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCard(CardSet set)
        {
            return cardPool.Where(c => c.InSet(set)).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCard(CardType type)
        {
            return cardPool.Where(c => c.Type == type).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCard(Card card)
        {
            return cardPool.Where(c => c != card).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCard(IList<Card> cards)
        {
            return cardPool.Where(c => !cards.Contains(c)).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }
    }

    public class PickerSettings
    {
        [XmlIgnore]
        public List<SetSelector> Sets { get; set; }

        public List<CardSet> SelectedSets
        {
            get
            {
                return Sets.Where(s => s.IsSelected).Select(s => s.Set).ToList();
            }
            set
            {
                Sets = Cards.AllSets.Select(s => new SetSelector(s, value.Contains(s))).ToList();
            }
        }

        public PickerOption MinimumCardsPerSet = new IntPickerOption("Minimum cards per set", 3, Enumerable.Range(1, 10).ToList());
        public PickerOption RequirePlusActions = new PickerOption("Require a card that gives actions", false);
        public PickerOption RequirePlusBuys = new PickerOption("Require a card that gives buys", false);
        public PickerOption RequireDefense = new PickerOption("If there's an attack,\nrequire a defense card", false);

        public List<PickerOption> AllOptions
        {
            get
            {
                return new List<PickerOption> 
                { 
                    MinimumCardsPerSet,
                    RequirePlusActions,
                    RequirePlusBuys,
                    RequireDefense,
                };

            }
        }

        public PickerSettings() 
        {
            Sets = Cards.AllSets.Select(s => new SetSelector(s)).ToList();
        }

        public PickerSettings Clone()
        {
            PickerSettings clone = new PickerSettings();
            clone.Sets = this.Sets;
            clone.MinimumCardsPerSet = this.MinimumCardsPerSet.Clone();
            clone.RequirePlusActions = this.RequirePlusActions.Clone();
            clone.RequirePlusBuys = this.RequirePlusBuys.Clone();
            clone.RequireDefense = this.RequireDefense.Clone();

            return clone;
        }
    }

    public class SetSelector
    {
        public CardSet Set { get; set; }
        public Boolean IsSelected { get; set; }

        public SetSelector(CardSet set) : this(set, true) { }
        public SetSelector(CardSet set, Boolean selected)
        {
            this.Set = set;
            this.IsSelected = selected;
        }
    }
}

