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
        private List<Card> cardPool
        {
            get
            {
                return Cards.AllCards.Where(c => c.InSet(PickerState.Current.CurrentSettings.SelectedSets)).ToList();
            }
        }
        
        public ObservableCollection<Card> GenerateCardList()
        {
            return GenerateCardList(PickerState.Current.CurrentSettings);
        }

        public ObservableCollection<Card> GenerateCardList(PickerSettings settings)
        {
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
}

