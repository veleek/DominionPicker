using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace Ben.Dominion
{
    public class Picker
    {
        private List<Card> cardSet { get; set; }
        private List<Card> cardPool
        {
            get
            {
                return Cards.AllCards.Where(c => c.InSet(Settings.SelectedSets)).ToList();
            }
        }

        public PickerSettings Settings { get { return PickerState.Current.CurrentSettings; } }
        
        public ObservableCollection<Card> GenerateCardList()
        {
            return GenerateCardList(PickerState.Current.CurrentSettings);
        }

        public ObservableCollection<Card> GenerateCardList(PickerSettings settings)
        {
            Regex actionRegex = new Regex("\\+. Action");
            Regex buyRegex = new Regex("\\+. Buy");

            Int32 minCards = settings.MinimumCardsPerSet.SelectedValue;
            Int32 maxSets = (Int32)Math.Floor(10 / minCards);
            var availableSets = Settings.SelectedSets.OrderBy(s => Guid.NewGuid()).Take(maxSets).ToList();
            List<Card> pool = Cards.AllCards.Where(c => c.InSet(availableSets)).ToList();

            do
            {
                cardSet = pool.OrderBy(c => Guid.NewGuid()).Take(10).ToList();

                if ((Boolean)Settings.RequirePlusActions.OptionValue)
                {
                    if (!cardSet.Any(c => actionRegex.IsMatch(c.Rules)))
                    {
                        continue;
                    }
                }

                if((Boolean)Settings.RequirePlusBuys.OptionValue)
                {
                    if(!cardSet.Any(c=> buyRegex.IsMatch(c.Rules)))
                    {
                        continue;
                    }
                }

                break;
            } 
            while (true);

            // Order them alphabetically
            cardSet = cardSet.OrderBy(c => c.Name).ToList();

            return cardSet.ToObservableCollection();
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
}

