using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class Picker
    {
        private List<Card> cardSet;
        public List<Card> pool;
        private Boolean isGenerating;
        private Boolean generationCanceled;

        public Picker()
        {
            // For ease of use, just initialize the card pool to all the cards
            pool = Cards.AllCards.OrderBy(c => Guid.NewGuid()).ToList();
        }

        public List<Card> GetCardPool()
        {
            return GetCardPool(PickerState.Current.CurrentSettings);
        }

        public List<Card> GetCardPool(PickerSettings settings)
        {
            return Cards.AllCards.Where(c => !c.IsType(settings.FilteredTypes)) // Filter out all of the unwanted card types
                                         //.Where(c => c.InSet(availableSets)) // Then filter to those in the available sets
                                         .Where(c => settings.FilterPotions.IsEnabled ? !c.HasPotion : true)
                                         .OrderBy(c => Guid.NewGuid()) // The order randomly
                                         .ToList();
        }

        public PickerResult GenerateCardList()
        {
            return GenerateCardList(PickerState.Current.CurrentSettings);
        }

        public PickerResult GenerateCardList(PickerSettings settings)
        {
            // This is the number of cards to generate in the set
            Int32 count = 10;
            // Reperesents the number of times we've tried to find a set
            Int32 creationAttempts = 0;
            Int32 minimumCardsPerSet = 0;

            List<CardSet> availableSets = settings.SelectedSets;
            PickerResult result = new PickerResult();

            generationCanceled = false;

            try
            {
                isGenerating = true;

                do
                {
                    // Allows fast fail
                    if (generationCanceled)
                    {
                        return null;
                    }

                    creationAttempts++;

                    // Create an empty result set
                    cardSet = new List<Card>();

                    if (settings.MinimumCardsPerSet.IsEnabled)
                    {
                        minimumCardsPerSet = settings.MinimumCardsPerSet.SelectedValue;
                        Int32 maxSets = (Int32)Math.Floor(count / minimumCardsPerSet);
                        availableSets = availableSets.OrderBy(s => Guid.NewGuid()).Take(maxSets).ToList();
                    }

                    pool = Cards.AllCards.Where(c => !c.IsType(settings.FilteredTypes)) // Filter out all of the unwanted card types
                                         .Where(c => c.InSet(availableSets)) // Then filter to those in the available sets
                                         .Where(c => settings.FilterPotions.IsEnabled ? !c.HasPotion : true)
                                         .OrderBy(c => Guid.NewGuid()) // The order randomly
                                         .ToList();

                    if (settings.MinimumCardsPerSet.IsEnabled)
                    {
                        foreach (CardSet set in availableSets)
                        {
                            // Get the minimum cards reqd. from each set
                            MoveCards(pool.Where(c => c.InSet(set)).Take(minimumCardsPerSet));
                        }
                    }

                    // Then fill up the card set with random cards
                    MoveCards(pool.Take(10 - cardSet.Count));

                    // Put the cards in the result to get access to all the properties we want
                    result.Cards = cardSet.ToObservableCollection();

                    if (cardSet.Count < 10)
                    {
                        // We've taken all the cards we can so just return 
                        // what we've got
                        break;
                    }

                    // Do other specific things, i.e. check if we need provinces and/or curses, or pick a bane card

                    // If there are any attacks and no defense, veto this set
                    if (settings.RequireDefense.IsEnabled && result.HasAttack && !result.HasReaction)
                    {
                        continue;
                    }

                    if (settings.RequireTrash.IsEnabled && !result.HasTrash)
                    {
                        continue;
                    }

                    if (settings.PlusActions.IsEnabled)
                    {
                        if (result.HasPlus2Action)
                        {
                            if (settings.PlusActions.Is("Prevent +2"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (settings.PlusActions.Is("Require +2"))
                            {
                                continue;
                            }
                        }

                        if (result.HasPlusAction)
                        {
                            if (settings.PlusActions.Is("Prevent"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (settings.PlusActions.Is("Require"))
                            {
                                continue;
                            }
                        }
                    }

                    if (settings.PlusBuys.IsEnabled)
                    {
                        if (result.HasPlus2Buy)
                        {
                            if (settings.PlusBuys.Is("Prevent +2"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (settings.PlusBuys.Is("Require +2"))
                            {
                                continue;
                            }
                        }

                        if (result.HasPlusBuy)
                        {
                            if (settings.PlusBuys.Is("Prevent"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (settings.PlusBuys.Is("Require"))
                            {
                                continue;
                            }
                        }
                    }

                    // We passed all the checks, so return the set
                    break;
                }
                while (true);
            }
            finally
            {
                isGenerating = false;
            }

            AppLog.Instance.Log(String.Format("Completed in {0} tries.", creationAttempts));

            return result;
        }

        public PickerResult GenerateCardListNew(PickerSettings settings)
        {
            PickerResult result = new PickerResult();
            Int32 creationAttempts = 0;
            List<CardSet> availableSets = settings.SelectedSets;

            generationCanceled = false;

            // Allows fast fail
            if (generationCanceled)
            {
                return null;
            }

            cardSet = new List<Card>();

            // This is the number of cards to generate in the set
            Int32 count = 10;
            Int32 minimumCardsPerSet = 0;
            
            if (settings.MinimumCardsPerSet.IsEnabled)
            {
                minimumCardsPerSet = settings.MinimumCardsPerSet.SelectedValue;
                Int32 maxSets = (Int32)Math.Floor(count / minimumCardsPerSet);
                availableSets = availableSets.OrderBy(s => Guid.NewGuid()).Take(maxSets).ToList();
            }


            pool = Cards.AllCards.Where(c => !c.IsType(settings.FilteredTypes)) // Filter out all of the unwanted card types
                                 .Where(c => c.InSet(availableSets)) // Then filter to those in the available sets
                                 .OrderBy(c => Guid.NewGuid()) // The order randomly
                                 .ToList();


            /*
            while (cardSet.Count < count)
            {
                // Pick a card
                Card c = pool[0];
            }
             */
                

            creationAttempts++;

            AppLog.Instance.Log(String.Format("Generated card set"));

            return result;
        }

        public void CancelGeneration()
        {
            generationCanceled = true;

            while (isGenerating)
            {
                System.Threading.Thread.Sleep(100);
            }
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
            var cardNames = cards.Select(c => c.Name);
            return pool.Where(c => !cardNames.Contains(c.Name)).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }
    }
}

