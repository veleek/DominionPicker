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
        private List<Card> pool;
        private Boolean generationCanceled;

        public PickerSettings Settings { get { return PickerState.Current.CurrentSettings; } }

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
            List<CardSet> availableSets = null;
            Int32 creationAttempts = 0;

            PickerResult result = new PickerResult();

            generationCanceled = false;

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

                result.Cards = cardSet.OrderBy(c => c.Name).ToObservableCollection();

                if (result.Cards.Count < 10)
                {
                    // We have less than 10 cards total so just return
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

            AppLog.Instance.Log(String.Format("Completed in {0} tries.", creationAttempts));

            return result;
        }

        public void CancelGeneration()
        {
            generationCanceled = true;
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

