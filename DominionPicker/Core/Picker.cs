using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ben.Utilities;

namespace Ben.Dominion
{
    /// <summary>
    /// Contains all the logic required to generate a set of cards from the full card pool
    /// </summary>
    public class Picker
    {
        private static readonly Random random = new Random();

        private Boolean isGenerating;
        private Boolean generationCanceled;

        public Picker()
        {
        }

        public PickerResult GenerateCardList(SettingsViewModel settings)
        {
            return this.GenerateCardList(settings, ResultSortOrder.Name);
        }

        public PickerResult GenerateCardList(SettingsViewModel settings, ResultSortOrder sortOrder)
        {
            // This is the number of cards to generate in the set
            const Int32 count = 10;
            // Reperesents the number of times we've tried to find a set
            Int32 creationAttempts = 0;

            List<CardSet> availableSets = settings.SelectedSets;
            Int32 minimumCardsPerSet = settings.MinimumCardsPerSet.OptionValue;
            Int32 maxSets = (Int32) Math.Floor((double) count / minimumCardsPerSet);

            PickerResult result = new PickerResult();

            this.generationCanceled = false;

            try
            {
                this.isGenerating = true;

                do
                {
                    // Allows fast fail
                    if (this.generationCanceled)
                    {
                        return null;
                    }

                    creationAttempts++;

                    // 1. Initialize the card pool   
                    {
                        if (settings.MinimumCardsPerSet.Enabled)
                        {
                            availableSets = availableSets.OrderBy(s => Guid.NewGuid()).Take(maxSets).ToList();
                        }

                        // The minimum set of pre-filtering we should do.
                        result.Pool = Cards.PickableCards.Where(c => c.InSet(availableSets))
                            .Where(c => !settings.FilteredCards.Ids.Contains(c.ID))
                            .OrderBy(c => Guid.NewGuid())
                            .ToCardList();
                    }

                    // 2. Generate a set of cards
                    {
                        List<Card> cardSet = new List<Card>();

                        if (settings.MinimumCardsPerSet.Enabled)
                        {
                            foreach (CardSet set in availableSets)
                            {
                                // Get the minimum cards reqd. from each set
                                result.Pool.Where(c => c.InSet(set)).Take(minimumCardsPerSet).Move(result.Pool, cardSet);
                            }
                        }

                        // Then fill up the card set with random cards
                        result.Pool.Take(10 - cardSet.Count).Move(result.Pool, cardSet);

                        // Put the cards in the result to get access to all the properties we want
                        result.Cards = cardSet.ToCardList();
                    }

                    // 3. Verify the validity of the set, if it's not valid, generate another
                    // If we have <= 10 cards, there are no other possible sets to generate
                    // so don't look at any of the options just use this set
                    if (result.Cards.Count >= 10)
                    {
                        if (!this.CheckResultValidity(settings, result))
                        {
                            continue;
                        }
                    }

                    // 4. Add any additional cards/tokens/etc. that we might need
                    this.AddAdditionalCards(settings, result);

                    break;
                } while (true);
            }
            finally
            {
                this.isGenerating = false;
            }

            AppLog.Instance.Log(String.Format("Completed in {0} tries.", creationAttempts));

            result.SortOrder = sortOrder != ResultSortOrder.None ? sortOrder : ResultSortOrder.Name;
            return result;
        }

        /// <summary>
        /// This is an experimental version of the card list generation in an attempt to 
        /// be more efficient.  It is currently untested and non-functional
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public PickerResult GenerateCardListNew(SettingsViewModel settings)
        {
            PickerResult result = new PickerResult();
            Int32 creationAttempts = 0;
            List<CardSet> availableSets = settings.SelectedSets;

            this.generationCanceled = false;

            // Allows fast fail
            if (this.generationCanceled)
            {
                return null;
            }

            List<Card> cardSet = new List<Card>();

            // This is the number of cards to generate in the set
            const int count = 10;

            if (settings.MinimumCardsPerSet.Enabled)
            {
                Int32 minimumCardsPerSet = settings.MinimumCardsPerSet.OptionValue;
                Int32 maxSets = (Int32) Math.Floor((double) count / minimumCardsPerSet);
                availableSets = availableSets.OrderBy(s => Guid.NewGuid()).Take(maxSets).ToList();
            }


            result.Pool =
                Cards.AllCards.Where(c => c.InSet(availableSets)) // Then filter to those in the available sets
                    .OrderBy(c => Guid.NewGuid()) // The order randomly
                    .ToCardList();


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

        /// <summary>
        /// Check if the selected set of cards meets the minimum bar to be a valid
        /// result set.
        /// </summary>
        /// <param name="settings">The settings used to determine if the result is valid</param>
        /// <param name="result">The result to check for validity</param>
        /// <returns>True if the cardset is valid based on the settings</returns>
        public bool CheckResultValidity(SettingsViewModel settings, PickerResult result)
        {
            // Do other specific things, i.e. check if we need provinces and/or curses, or pick a bane card

            // If there are any attacks and no defense, veto this set
            if (settings.RequireDefense && result.HasAttack && !result.HasReaction)
            {
                return false;
            }

            if (settings.RequireTrash && !result.HasTrash)
            {
                return false;
            }

            if (settings.PlusActions.Enabled)
            {
                if (result.HasPlus2Action)
                {
                    if (settings.PlusActions.Is("Prevent +2"))
                    {
                        return false;
                    }
                }
                else
                {
                    if (settings.PlusActions.Is("Require +2"))
                    {
                        return false;
                    }
                }

                if (result.HasPlusAction)
                {
                    if (settings.PlusActions.Is("Prevent"))
                    {
                        return false;
                    }
                }
                else
                {
                    if (settings.PlusActions.Is("Require"))
                    {
                        return false;
                    }
                }
            }

            if (settings.PlusBuys.Enabled)
            {
                if (result.HasPlus2Buy)
                {
                    if (settings.PlusBuys.Is("Prevent +2"))
                    {
                        return false;
                    }
                }
                else
                {
                    if (settings.PlusBuys.Is("Require +2"))
                    {
                        return false;
                    }
                }

                if (result.HasPlusBuy)
                {
                    if (settings.PlusBuys.Is("Prevent"))
                    {
                        return false;
                    }
                }
                else
                {
                    if (settings.PlusBuys.Is("Require"))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check if there are any additional cards that need to be added based
        /// on the rules picked and the cards in the result
        /// </summary>
        /// <param name="settings">The settings which define what additional cards to add</param>
        /// <param name="result">The result to add the additional cards to</param>
        public void AddAdditionalCards(SettingsViewModel settings, PickerResult result)
        {
            result.AdditionalCards = new CardList();

            if (result.HasCard("Young Witch"))
            {
                
                // TODO: There is a bug where a bane card doesn't get selected occasionally.
                // This likely occurs when the 'pool' of cards doesn't contain any cost 2-3 cards.

                Card bane = result.Pool.FirstOrDefault(c => c.Cost == "2" || c.Cost == "3");

                if (bane != null)
                {
                    result.Pool.Remove(bane);
                    // Todo: Localize this string
                    result.AdditionalCards.Add(bane.WithLabel("bane for Young Witch"));
                }
                else
                {
                    result.AdditionalStuff.Add("No cards available for Bane in selected sets");
                }
            }

            // The rest of these cards are non-pickable cards so they don't need to be moved 
            // from the pool and can just be added directly.

            if (settings.PickPlatinumColony)
            {
                // Pick a random card from the pool
                Card colonyPlatinumCard = result.Cards[random.Next(result.Cards.Count)];

                // If it's a prosperty card then we use Colony and Platinum
                if (colonyPlatinumCard.Set == CardSet.Prosperity)
                {
                    // 70% of the time we just use both
                    if (random.NextDouble() <= 0.7)
                    {
                        result.AdditionalCards.Add(Card.FromName("Platinum").WithLabel("selected because of Prosperity"));
                        result.AdditionalCards.Add(Card.FromName("Colony"));
                    }
                    else
                    {
                        // Otherwise randomly pick one of them.
                        switch (random.Next(2))
                        {
                            case 0:
                                result.AdditionalCards.Add(Card.FromName("Platinum").WithLabel("selected because of Prosperity"));
                                break;
                            case 1:
                                result.AdditionalCards.Add(Card.FromName("Colony").WithLabel("selected because of Prosperity"));
                                break;
                        }
                    }
                }
            }

            if (settings.PickShelterOrEstate)
            {
                Card shelterEstateCard = result.Cards[random.Next(result.Cards.Count)];

                if (shelterEstateCard.InSet(CardSet.DarkAges))
                {
                    result.AdditionalCards.Add(Card.FromName("Shelters").WithLabel("selected because of Dark Ages"));
                }
            }

	        var requireLooter = result.CardsOfType(CardType.Looter).ToList();
            if (requireLooter.Any())
            {
				result.AdditionalCards.Add(Card.FromName("Ruins").WithLabel("required by " + string.Join(",", requireLooter.Select(c => c.DisplayName))));
            }

            // Is this any better, or should we prefer being explicit.
            if(result.Cards.Any(c => c.ContainsText("Spoils")))
            //if (result.HasCard("Bandit Camp") || result.HasCard("Marauder") || result.HasCard("Pillage"))
            {
                result.AdditionalCards.Add(Card.FromName("Spoils"));
            }

            if (result.HasCard("Hermit"))
            {
                result.AdditionalCards.Add(Card.FromName("Madman").WithLabel("required by Hermit"));
            }

            if (result.HasCard("Urchin"))
            {
                result.AdditionalCards.Add(Card.FromName("Mercenary").WithLabel("required by Mercenary"));
            }

            if (result.HasCard("Page"))
            {
                result.AdditionalCards.Add(Card.FromName("Treasure Hunter").WithLabel("required by Page"));
                result.AdditionalCards.Add(Card.FromName("Warrior"));
                result.AdditionalCards.Add(Card.FromName("Hero"));
                result.AdditionalCards.Add(Card.FromName("Champion"));
            }

	        if (result.HasCard("Peasant"))
	        {
		        result.AdditionalCards.Add(Card.FromName("Soldier").WithLabel("required by Peasant"));
		        result.AdditionalCards.Add(Card.FromName("Fugitive"));
		        result.AdditionalCards.Add(Card.FromName("Disciple"));
		        result.AdditionalCards.Add(Card.FromName("Teacher"));
	        }

	        if (result.Cards.Any(c => c.HasPotion))
            {
                result.AdditionalCards.Add(Card.FromName("Potion"));
            }

            var requireCurse = result.CardsWhere(c => c.ContainsText("Curse")).ToArray();
            if (requireCurse.Any())
            {
                var curse = Card.FromName("Curse").Clone();
                curse.Label = string.Format("required by {0}", string.Join(", ", requireCurse.Select(c => c.DisplayName)));
                result.AdditionalCards.Add(curse);
            }

            if (settings.ShowExtras)
            {
                // Now check for additional 'stuff' like mats and tokens
                List<string> additionalStuff = new List<string>();

                if (result.HasCard("Native Village"))
                {
                    additionalStuff.Add("Native Village Mat");
                }

                if (result.HasCard("Island"))
                {
                    additionalStuff.Add("Island Mat");
                }

                if (result.HasCard("Pirate Ship"))
                {
                    additionalStuff.Add("Pirate Ship Mat");
                }
                
                if (result.HasCard("Trade Route"))
                {
                    additionalStuff.Add("Trade Route Mat");
                }

                if (result.HasCardType(CardType.Reserve))
                {
                    additionalStuff.Add("Tavern Mat");
                }

                if (result.HasCard("Embargo"))
                {
                    additionalStuff.Add("Embargo Tokens");
                }

                if (result.HasCard("Baker") ||
                    result.HasCard("Butcher") ||
                    result.HasCard("Candlestick Maker") ||
                    result.HasCard("Merchant Guild") ||
                    result.HasCard("Pirate Ship") ||
                    result.HasCard("Plaza") ||
                    result.HasCard("Trade Route"))
                {
                    additionalStuff.Add("Coin Tokens");
                }

                if (result.HasCard(c => c.ContainsText("{VP}")))
                {
                    additionalStuff.Add("Victory Point Tokens");
                }

                result.AdditionalStuff = additionalStuff.Distinct().OrderBy(s => s).ToList();
            }
        }

        /// <summary>
        /// Cancel the current generation loop
        /// </summary>
        public void CancelGeneration()
        {
            this.generationCanceled = true;

            while (this.isGenerating)
            {
                Thread.Sleep(100);
            }
        }
    }
}