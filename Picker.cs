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
        private List<Card> cardSet;
        private List<Card> pool;

        public PickerSettings Settings { get { return PickerState.Current.CurrentSettings; } }

        public Picker()
        {
            // For ease of use, just initialize the card pool to all the cards
            pool = Cards.AllCards.OrderBy(c => Guid.NewGuid()).ToList();
        }

        public ObservableCollection<Card> GenerateCardList()
        {
            return GenerateCardList(PickerState.Current.CurrentSettings);
        }

        public ObservableCollection<Card> GenerateCardList(PickerSettings settings)
        {
            Regex actionRegex = new Regex("\\+. Action");
            Regex buyRegex = new Regex("\\+. Buy");

            List<CardSet> availableSets = null;
            Int32 creationAttempts = 0;
            
            do
            {
                creationAttempts++;
                
                // Create an empty result set
                cardSet = new List<Card>();

                // Reset the card pool and filter out all the unwanted types
                var tempPool = Cards.AllCards.Where(c => c.IsType(settings.SelectedTypes));

                Int32 minCards = settings.MinimumCardsPerSet.SelectedValue;

                if (settings.MinimumCardsPerSet.IsEnabled)
                {
                    Int32 maxSets = (Int32)Math.Floor(10 / minCards);
                    availableSets = Settings.SelectedSets.OrderBy(s => Guid.NewGuid()).Take(maxSets).ToList();
                }
                else
                {
                    availableSets = Settings.SelectedSets;
                }

                // Filter the pool to cards from all the selected sets and order them randomly
                pool = tempPool.Where(c => c.InSet(availableSets)).OrderBy(c => Guid.NewGuid()).ToList();

                if(settings.MinimumCardsPerSet.IsEnabled)
                {
                    foreach (CardSet set in availableSets)
                    {
                        // Get the minimum cards reqd. from each set
                        MoveCards(pool.Where(c => c.InSet(set)).Take(minCards));
                    }

                    MoveCards(pool.Take(10 - cardSet.Count));
                }
                else
                {
                    // Pick 10 at random from the pool
                    MoveCards(pool.Take(10));
                }

                if (cardSet.Count < 10)
                {
                    // We have less than 10 cards total so just return
                    break;
                }

                if (Settings.RequireDefense.IsEnabled)
                {
                    // If there are any attacks and no defense, veto this set
                    if (cardSet.Any(c => c.IsType(CardType.Attack)) && !cardSet.Any(c => c.IsType(CardType.Reaction)))
                    {
                        continue;
                    }
                }

                if (Settings.PlusActions.IsEnabled)
                {
                    // Check if there are any +Actions cards
                    Boolean hasPlusAction = cardSet.Any(c => actionRegex.IsMatch(c.Rules));

                    // If a plus actions card is required and not present or prevented and present
                    // throw out the set and try again.
                    if (Settings.PlusActions.IsRequired ^ hasPlusAction)
                    {
                        continue;
                    }
                }

                if (Settings.PlusBuys.IsEnabled)
                {
                    // Check if there are any +Buys cards
                    Boolean hasPlusBuy = cardSet.Any(c => buyRegex.IsMatch(c.Rules));

                    // If a plus buys card is required and not present or prevented and present
                    // throw out the set and try again.
                    if (Settings.PlusBuys.IsRequired ^ hasPlusBuy)
                    {
                        continue;
                    }
                }

                // We passed all the checks, so return the set
                break;
            }
            while (true);

            // Order them alphabetically
            cardSet = cardSet.OrderBy(c => c.Name).ToList();

            AppLog.Instance.Log(String.Format("Completed in {0} tries.", creationAttempts));

            return cardSet.ToObservableCollection();
        }

        private void MoveCards(IEnumerable<Card> cards)
        {
            foreach (Card c in cards.ToList())
            {
                cardSet.Add(c);
                pool.Remove(c);
            }
        }

        public Card GetRandomCard()
        {
            return pool.OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCard(CardSet set)
        {
            return pool.Where(c => c.InSet(set)).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCard(CardType type)
        {
            return pool.Where(c => c.Type == type).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCard(Card card)
        {
            return pool.Where(c => c != card).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public Card GetRandomCardOtherThan(IList<Card> cards)
        {
            return pool.Where(c => !cards.Contains(c)).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }
    }
}

