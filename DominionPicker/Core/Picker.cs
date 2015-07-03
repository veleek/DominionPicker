using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ben.Dominion.ViewModels;
using Ben.Utilities;
using Ben.Dominion.Models;

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

            List<CardSet> availableSets;
            Int32 minimumCardsPerSet = settings.MinimumCardsPerSet.Enabled ? settings.MinimumCardsPerSet.OptionValue : 1;
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

                    // 1. Select the sets that are going to be in the result
                    {
                        // Starting with the pinned sets take as many as we can
                        availableSets = settings.Sets
                            // If Enabled is null, then we are in the pinned state
                            .Where(s => s.Enabled == null)
                            .Select(s => s.Set)
                            .OrderBy(s => Guid.NewGuid())
                            .Take(maxSets)
                            .ToList();

                        // Fill in the remainder with random sets 
                        for (int i = availableSets.Count; i < maxSets; i++)
                        {
                            availableSets.Add(settings.SelectedSets[random.Next(settings.SelectedSets.Count)]);
                        }

                        availableSets = availableSets.Distinct().ToList();
                    }

                    // 2. Initialize the card pool   
                    {
                        result.Pool = Cards.PickableCards.Where(c => c.InSet(availableSets))
                            .Where(c => !settings.FilteredCards.Ids.Contains(c.ID))
                            .OrderBy(c => Guid.NewGuid())
                            .ToCardList();
                    }

                    // 3. Generate a set of cards
                    {
                        List<Card> cardSet = new List<Card>();

                        // For each of the sets in the result, take the minimum number of cards 
                        foreach (var set in availableSets)
                        {
                            result.Pool.Where(c => c.InSet(set)).Take(minimumCardsPerSet).Move(result.Pool, cardSet);
                        }
                        
                        // Then fill up the card set with random cards
                        result.Pool.Take(10 - cardSet.Count).Move(result.Pool, cardSet);

                        // Put the cards in the result to get access to all the properties we want
                        CardGroup kingdomCard = new CardGroup(CardGroupType.KingdomCard);
                        result.Cards = cardSet.Select(c => c.WithGroup(kingdomCard)).ToCardList();
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
            if (settings.RequireDefense && result.HasAttack && !result.HasReactionOrLighthouse)
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
                    result.Cards.Add(bane.WithGroup(new CardGroup(CardGroupType.BaneRequired, Card.FromName("Young Witch"))));
                }
                else
                {
                    result.AdditionalStuff.Add("No card available for Bane in selected sets");
                }
            }

            // The rest of these cards are non-pickable cards so they don't need to be moved 
            // from the pool and can just be added directly.

            if (ConfigurationModel.Instance.PickPlatinumColony != PlatinumColonyOption.Never)
            {
                // Pick a random card from the pool
                Card colonyPlatinumCard = result.Cards[random.Next(result.Cards.Count)];

                // If it's a prosperty card then we use Colony and Platinum
                if (ConfigurationModel.Instance.PickPlatinumColony == PlatinumColonyOption.Always || colonyPlatinumCard.InSet(CardSet.Prosperity))
                {
                    CardGroup prosperityGroup = new CardGroup(CardGroupType.SelectedProsperity);
                    Card colony = Card.FromName("Colony").WithGroup(prosperityGroup);
                    Card platinum = Card.FromName("Platinum").WithGroup(prosperityGroup);

                    result.Cards.Add(platinum);
                    result.Cards.Add(colony);
                }
            }

            if (ConfigurationModel.Instance.PickSheltersOrEstates != SheltersOption.Never)
            {
                Card shelterEstateCard = result.Cards[random.Next(result.Cards.Count)];

                if (ConfigurationModel.Instance.PickSheltersOrEstates == SheltersOption.Always || shelterEstateCard.InSet(CardSet.DarkAges))
                {
                    result.Cards.Add(Card.FromName("Shelters").WithGroup(new CardGroup(CardGroupType.SelectedDarkAges)));
                }
            }

	        var requireLooter = result.CardsOfType(CardType.Looter).ToList();
            if (requireLooter.Any())
            {
				result.Cards.Add(Card.FromName("Ruins").WithGroup(new CardGroup(CardGroupType.OtherRequired, requireLooter)));
            }

            var requireSpoils = result.Cards.Where(c => c.ContainsText("Spoils")).ToList();
            // Is this any better, or should we prefer being explicit.
            // var requireSpoils = (result.HasCard("Bandit Camp") || result.HasCard("Marauder") || result.HasCard("Pillage"))
            if (requireSpoils.Any())
            {
                result.Cards.Add(Card.FromName("Spoils").WithGroup(new CardGroup(CardGroupType.OtherRequired, requireSpoils)));
            }

            if (result.HasCard("Hermit"))
            {
                result.Cards.Add(Card.FromName("Madman").WithGroup(new CardGroup(CardGroupType.OtherRequired, Card.FromName("Hermit"))));
            }

            if (result.HasCard("Urchin"))
            {
                result.Cards.Add(Card.FromName("Mercenary").WithGroup(new CardGroup(CardGroupType.OtherRequired, Card.FromName("Urchin"))));
            }

            if (result.HasCard("Page"))
            {
                var pageGroup = new CardGroup(CardGroupType.OtherRequired, Card.FromName("Page"));
                result.Cards.Add(Card.FromName("Treasure Hunter").WithGroup(pageGroup));
                result.Cards.Add(Card.FromName("Warrior").WithGroup(pageGroup));
                result.Cards.Add(Card.FromName("Hero").WithGroup(pageGroup));
                result.Cards.Add(Card.FromName("Champion").WithGroup(pageGroup));
            }

	        if (result.HasCard("Peasant"))
	        {
                var peasantGroup = new CardGroup(CardGroupType.OtherRequired, Card.FromName("Peasant"));
                result.Cards.Add(Card.FromName("Soldier").WithGroup(peasantGroup));
		        result.Cards.Add(Card.FromName("Fugitive").WithGroup(peasantGroup));
                result.Cards.Add(Card.FromName("Disciple").WithGroup(peasantGroup));
                result.Cards.Add(Card.FromName("Teacher").WithGroup(peasantGroup));
            }

            var requirePotion = result.Cards.Where(c => c.HasPotion).ToList();
            if (requirePotion.Any())
            {
                result.Cards.Add(Card.FromName("Potion").WithGroup(new CardGroup(CardGroupType.OtherRequired, requirePotion)));
            }

            var requireCurse = result.CardsWhere(c => c.ContainsText("Curse")).ToList();
            if (requireCurse.Any())
            {
                var curse = Card.FromName("Curse");
                result.Cards.Add(curse.WithGroup(new CardGroup(CardGroupType.OtherRequired, requireCurse)));
            }

            if (ConfigurationModel.Instance.ShowExtras)
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

                if (result.HasCard(c => !c.IsType(CardType.Victory) && c.ContainsText("{VP}")))
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