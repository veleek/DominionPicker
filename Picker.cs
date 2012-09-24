using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Ben.Utilities;
using System.Diagnostics;

namespace Ben.Dominion
{
    /// <summary>
    /// Contains all the logic required to generate a set of cards from the full card pool
    /// </summary>
    public class Picker
    {
        private List<Card> cardSet;
        public List<Card> pool;
        private Boolean isGenerating;
        private Boolean generationCanceled;
        private Random random = new Random();

        public Picker()
        {
            // For ease of use, just initialize the card pool to all the cards
            pool = Cards.PickableCards.OrderBy(c => Guid.NewGuid()).ToList();
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

                    result.AdditionalCards = new ObservableCollection<Card>();

                    // Create an empty result set
                    cardSet = new List<Card>();

                    if (settings.MinimumCardsPerSet.IsEnabled)
                    {
                        minimumCardsPerSet = settings.MinimumCardsPerSet.SelectedValue;
                        Int32 maxSets = (Int32)Math.Floor(count / minimumCardsPerSet);
                        availableSets = availableSets.OrderBy(s => Guid.NewGuid()).Take(maxSets).ToList();
                    }

                    pool = Cards.PickableCards.Where(c => !c.IsType(settings.FilteredTypes)) // Filter out all of the unwanted card types
                                         .Where(c => c.InSet(availableSets)) // Then filter to those in the available sets
                                         .Where(c => settings.FilterPotions.IsEnabled ? !c.HasPotion : true)
                                         .Where(c => !settings.FilteredCardIds.Contains(c.ID)) // Only grab the cards that aren't filtered
                                         .OrderBy(c => Guid.NewGuid()) // Then order randomly
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

                    // If we have less than 11 cards, there are no other possible sets to generate
                    // so don't look at any of the options just use this set
                    if (cardSet.Count >= 10)
                    {
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
                    }

                    // Select the bane card first so that it appears close to Young Witch
                    if (result.HasCard("Young Witch"))
                    {
                        Card bane = pool.FirstOrDefault(c => c.Cost == "2" || c.Cost == "3");
                        if (bane != null)
                        {
                            result.AdditionalCards.Add(bane);
                        }
                    }

                    if (settings.PickPlatinumColony.IsEnabled)
                    {
                        // Pick a random card from the pool
                        Card colonyPlatinumCard = result.Cards[random.Next(result.Cards.Count)];

                        // If it's a prosperty card then we use Colony and Platinum
                        if (colonyPlatinumCard.Set == CardSet.Prosperity)
                        {
                            // 70% of the time we just use both
                            if (random.NextDouble() <= 0.7)
                            {
                                result.AdditionalCards.Add(Card.FromName("Platinum"));
                                result.AdditionalCards.Add(Card.FromName("Colony"));
                            }
                            else
                            {
                                // Otherwise randomly pick one of them.
                                switch (random.Next(2))
                                {
                                    case 0:
                                        result.AdditionalCards.Add(Card.FromName("Platinum"));
                                        break;
                                    case 1:
                                        result.AdditionalCards.Add(Card.FromName("Colony"));
                                        break;
                                }
                            }
                        }
                    }

                    if (settings.PickSheltersOrEstates.IsEnabled)
                    {
                        Card shelterEstateCard = result.Cards[random.Next(result.Cards.Count)];

                        if (shelterEstateCard.InSet(CardSet.DarkAges))
                        {
                            Debug.WriteLine("Adding shelters...");
                            result.AdditionalCards.Add(Card.FromName("Shelters"));
                            //foreach (var shelter in Cards.AllCards.Where(c => c.IsType(CardType.Shelter)))
                            //{
                            //    result.AdditionalCards.Add(shelter);
                            //}
                        }
                    }

                    if(result.HasCardType(CardType.Looter))
                    {
                        Debug.WriteLine("Adding ruins...");
                        result.AdditionalCards.Add(Card.FromName("Ruins"));
                    }



                    if (result.HasCard("Bandit Camp") || result.HasCard("Marauder"))
                    {
                        result.AdditionalCards.Add(Card.FromName("Spoils"));
                    }

                    if(result.HasCard("Hermit"))
                    {
                        result.AdditionalCards.Add(Card.FromName("Madman"));
                    }

                    if (result.HasCard("Urchin"))
                    {
                        result.AdditionalCards.Add(Card.FromName("Mercenary"));
                    }

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

