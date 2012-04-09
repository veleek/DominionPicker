﻿using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class Picker : NotifyPropertyChangedBase
    {
        private List<Card> cardSet;
        private List<Card> pool;
        private Boolean isGenerating;
        private Boolean generationCanceled;

        public static PickerSettings Settings { get { return PickerState.Current.CurrentSettings; } }

        public Boolean IsGenerating
        {
            get
            {
                return isGenerating;
            }
            private set
            {
                if (value != isGenerating)
                {
                    isGenerating = value;
                    NotifyPropertyChanged("IsGenerating");
                }
            }
        }

        public Picker()
        {
            // For ease of use, just initialize the card pool to all the cards
            pool = Cards.AllCards.OrderBy(c => Guid.NewGuid()).ToList();
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

            List<CardSet> availableSets = Settings.SelectedSets;
            PickerResult result = new PickerResult();

            generationCanceled = false;

            try
            {
                IsGenerating = true;

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
                    if (Settings.RequireDefense.IsEnabled && result.HasAttack && !result.HasReaction)
                    {
                        continue;
                    }

                    if (Settings.RequireTrash.IsEnabled && !result.HasTrash)
                    {
                        continue;
                    }

                    if (Settings.PlusActions.IsEnabled)
                    {
                        if (result.HasPlus2Action)
                        {
                            if (Settings.PlusActions.Is("Prevent +2"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (Settings.PlusActions.Is("Require +2"))
                            {
                                continue;
                            }
                        }

                        if (result.HasPlusAction)
                        {
                            if (Settings.PlusActions.Is("Prevent"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (Settings.PlusActions.Is("Require"))
                            {
                                continue;
                            }
                        }
                    }

                    if (Settings.PlusBuys.IsEnabled)
                    {
                        if (result.HasPlus2Buy)
                        {
                            if (Settings.PlusBuys.Is("Prevent +2"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (Settings.PlusBuys.Is("Require +2"))
                            {
                                continue;
                            }
                        }

                        if (result.HasPlusBuy)
                        {
                            if (Settings.PlusBuys.Is("Prevent"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (Settings.PlusBuys.Is("Require"))
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
                IsGenerating = false;
            }

            AppLog.Instance.Log(String.Format("Completed in {0} tries.", creationAttempts));

            return result;
        }

        public PickerResult GenerateCardListNew(PickerSettings settings)
        {
            PickerResult result = new PickerResult();
            Int32 creationAttempts = 0;
            List<CardSet> availableSets = Settings.SelectedSets;

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

            while (IsGenerating)
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
            return pool.Where(c => !cards.Contains(c)).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }
    }
}

