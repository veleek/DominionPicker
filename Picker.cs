using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;

namespace Ben.Dominion
{
    public class Picker : INotifyPropertyChanged
    {
        public List<Card> CardSet { get; set; }

        private List<Card> cardPool;
        
        public void CreateCardList()
        {
            CreateCardList(PickerState.Current.CurrentSettings);
        }

        public void CreateCardList(PickerSettings settings)
        {
            cardPool = Cards.AllCards.Where(c => c.InSet(settings.SelectedSets)).ToList();
            CardSet = cardPool.OrderBy(c => Guid.NewGuid()).Take(10).ToList();
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


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }

    public class PickerSettings
    {
        public Dictionary<CardSet, Boolean> Sets { get; set; }
        public List<CardSet> SelectedSets
        {
            get
            {
                return Sets.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
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
            Sets = Cards.AllSets.ToDictionary(s => s, s => false);
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
}

