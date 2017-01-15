using System;
using System.Collections.Generic;
using System.Linq;
using Ben.Dominion.ViewModels;
using Ben.Utilities;
using System.Threading.Tasks;
//using GoogleAnalytics;

namespace Ben.Dominion
{
    /// <summary>
    /// Contains all the logic required to generate a set of cards from the full card pool
    /// </summary>
    public class Picker
    {
        private static readonly Random random = new Random();
        private static bool isGenerating;
        private static bool generationCanceled;

        public static PickerResult GenerateCardList(SettingsViewModel settings, ResultSortOrder sortOrder)
        {
            // This is the number of cards to generate in the set
            const Int32 count = 10;
            // Reperesents the number of times we've tried to find a set
            Int32 creationAttempts = 0;
            List<CardSet> availableSets;
            Int32 minimumCardsPerSet = settings.MinimumCardsPerSet.Enabled ? settings.MinimumCardsPerSet.OptionValue : 1;
            Int32 maxSets = (Int32)Math.Floor((double)count / minimumCardsPerSet);
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
                    
                    // 1. Select the sets that are going to be in the result
                    {
                        // Starting with the pinned sets take as many as we can
                        availableSets = settings.Sets
                                                .Where(s => s.Required)
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
                        result.Pool = Cards.PickableCards.Where(c => c.InSet(availableSets)).Where(c => !settings.FilteredCards.Ids.Contains(c.ID)).OrderBy(c => Guid.NewGuid()).ToCardList();
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
                        if (!CheckResultValidity(settings, result))
                        {
                            continue;
                        }
                    }
                    // 4. Add any additional cards/tokens/etc. that we might need
                    AddAdditionalCards(settings, result);
                    break;
                } while (true);
            }
            finally
            {
                isGenerating = false;
            }
            AppLog.Instance.Log(String.Format("Completed in {0} tries.", creationAttempts));
            //EasyTracker.GetTracker().SendEvent("Picker", "Generate Set", "Complete", creationAttempts);
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
        public static bool CheckResultValidity(SettingsViewModel settings, PickerResult result)
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

            switch (settings.PlusActionsOption)
            {
                case CardRequirementOption.Require:
                    if (!result.HasPlusAction)
                    {
                        return false;
                    }
                    break;
                case CardRequirementOption.RequirePlus2:
                    if (!result.HasPlus2Action)
                    {
                        return false;
                    }
                    break;
                case CardRequirementOption.Prevent:
                    if (result.HasPlusAction)
                    {
                        return false;
                    }
                    break;
                case CardRequirementOption.PreventPlus2:
                    if (result.HasPlus2Action)
                    {
                        return false;
                    }
                    break;
            }

            switch (settings.PlusBuysOption)
            {
                case CardRequirementOption.Require:
                    if (!result.HasPlusBuy)
                    {
                        return false;
                    }
                    break;
                case CardRequirementOption.RequirePlus2:
                    if (!result.HasPlus2Buy)
                    {
                        return false;
                    }
                    break;
                case CardRequirementOption.Prevent:
                    if (result.HasPlusBuy)
                    {
                        return false;
                    }
                    break;
                case CardRequirementOption.PreventPlus2:
                    if (result.HasPlus2Buy)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Check if there are any additional cards that need to be added based
        /// on the rules picked and the cards in the result
        /// </summary>
        /// <param name="settings">The settings which define what additional cards to add</param>
        /// <param name="result">The result to add the additional cards to</param>
        public static void AddAdditionalCards(SettingsViewModel settings, PickerResult result)
        {
            if(result.Cards.Count == 0)
            {
                return;
            }

            result.AdditionalStuff = new List<string>();
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
            {
                bool usePlatinumColony = false;
                switch (ConfigurationModel.Instance.PickPlatinumColony)
                {
                    case PlatinumColonyOption.Randomly:
                        usePlatinumColony = result.Cards[random.Next(result.Cards.Count)].InSet(CardSet.Prosperity);
                        break;
                    case PlatinumColonyOption.AlwaysWithProsperity:
                        usePlatinumColony = result.HasCard(c => c.InSet(CardSet.Prosperity));
                        break;
                    case PlatinumColonyOption.Always:
                        usePlatinumColony = true;
                        break;
                }
                if (usePlatinumColony)
                {
                    CardGroup prosperityGroup = new CardGroup(CardGroupType.SelectedProsperity);
                    result.Cards.Add((Card.FromName("Platinum")).WithGroup(prosperityGroup));
                    result.Cards.Add((Card.FromName("Colony")).WithGroup(prosperityGroup));
                }
            }

            {
                bool useShelters = false;
                switch (ConfigurationModel.Instance.PickSheltersOrEstates)
                {
                    case SheltersOption.Randomly:
                        useShelters = result.Cards[random.Next(result.Cards.Count)].InSet(CardSet.DarkAges);
                        break;
                    case SheltersOption.AlwaysWithDarkAges:
                        useShelters = result.HasCard(c => c.InSet(CardSet.Prosperity));
                        break;
                    case SheltersOption.Always:
                        useShelters = true;
                        break;
                }
                if (useShelters)
                {
                    result.Cards.Add((Card.FromName("Shelters")).WithGroup(new CardGroup(CardGroupType.SelectedDarkAges)));
                }
            }
            if (result.HasCard("Tournament"))
            {
                result.Cards.Add((Card.FromName("Prizes")).WithGroup(new CardGroup(CardGroupType.OtherRequired, Card.FromName("Tournament"))));
            }
            var requireLooter = result.CardsOfType(CardType.Looter).ToList();
            if (requireLooter.Any())
            {
                result.Cards.Add((Card.FromName("Ruins")).WithGroup(new CardGroup(CardGroupType.OtherRequired, requireLooter)));
            }
            var requireSpoils = result.Cards.Where(c => c.ContainsText("Spoils")).ToList();
            // Is this any better, or should we prefer being explicit.
            // var requireSpoils = (result.HasCard("Bandit Camp") || result.HasCard("Marauder") || result.HasCard("Pillage"))
            if (requireSpoils.Any())
            {
                result.Cards.Add((Card.FromName("Spoils")).WithGroup(new CardGroup(CardGroupType.OtherRequired, requireSpoils)));
            }
            if (result.HasCard("Hermit"))
            {
                result.Cards.Add((Card.FromName("Madman")).WithGroup(new CardGroup(CardGroupType.OtherRequired, Card.FromName("Hermit"))));
            }
            if (result.HasCard("Urchin"))
            {
                result.Cards.Add((Card.FromName("Mercenary")).WithGroup(new CardGroup(CardGroupType.OtherRequired, Card.FromName("Urchin"))));
            }
            if (result.HasCard("Page"))
            {
                var pageGroup = new CardGroup(CardGroupType.OtherRequired, Card.FromName("Page"));
                result.Cards.Add((Card.FromName("Treasure Hunter")).WithGroup(pageGroup));
                result.Cards.Add((Card.FromName("Warrior")).WithGroup(pageGroup));
                result.Cards.Add((Card.FromName("Hero")).WithGroup(pageGroup));
                result.Cards.Add((Card.FromName("Champion")).WithGroup(pageGroup));
            }
            if (result.HasCard("Peasant"))
            {
                var peasantGroup = new CardGroup(CardGroupType.OtherRequired, Card.FromName("Peasant"));
                result.Cards.Add((Card.FromName("Soldier")).WithGroup(peasantGroup));
                result.Cards.Add((Card.FromName("Fugitive")).WithGroup(peasantGroup));
                result.Cards.Add((Card.FromName("Disciple")).WithGroup(peasantGroup));
                result.Cards.Add((Card.FromName("Teacher")).WithGroup(peasantGroup));
            }
            var requirePotion = result.Cards.Where(c => c.HasPotion).ToList();
            if (requirePotion.Any())
            {
                result.Cards.Add((Card.FromName("Potion")).WithGroup(new CardGroup(CardGroupType.OtherRequired, requirePotion)));
            }
            var requireCurse = result.CardsWhere(c => c.ContainsText("Curse")).ToList();
            if (requireCurse.Any())
            {
                var curse = Card.FromName("Curse");
                result.Cards.Add(curse.WithGroup(new CardGroup(CardGroupType.OtherRequired, requireCurse)));
            }
            if (ConfigurationModel.Instance.PickEvents != EventsOption.Never)
            {
                bool hasEventsSet = result.HasCard(c => c.InSet(CardSet.Adventures) || c.InSet(CardSet.Empires));
                int numberOfEvents = 0;
                switch (ConfigurationModel.Instance.PickEvents)
                {
                    case EventsOption.Randomly:
                        numberOfEvents = random.Next(0, 3);
                        break;
                    case EventsOption.RandomlyWithSet:
                        if (hasEventsSet)
                        {
                            numberOfEvents = random.Next(0, 3);
                        }
                        break;
                    case EventsOption.Always:
                        numberOfEvents = 2;
                        break;
                    case EventsOption.AlwaysWithSet:
                        if (hasEventsSet)
                        {
                            numberOfEvents = 2;
                        }
                        break;
                }

                if (numberOfEvents > 0)
                {
                    var eventsGroup = new CardGroup(CardGroupType.Events);
                    var events = Cards.AllCards.Where(c => c.IsType(CardType.Event) || c.IsType(CardType.Landmark))
                                               .OrderBy(_ => Guid.NewGuid())
                                               .Take(numberOfEvents)
                                               .Select(e => e.WithGroup(eventsGroup));
                    result.Cards.AddRange(events);
                }
            }
            if (ConfigurationModel.Instance.ShowExtras)
            {
                if (result.HasCard("Native Village"))
                {
                    result.AdditionalStuff.Add("Native Village Mat");
                }
                if (result.HasCard("Island"))
                {
                    result.AdditionalStuff.Add("Island Mat");
                }
                if (result.HasCard("Pirate Ship"))
                {
                    result.AdditionalStuff.Add("Pirate Ship Mat");
                }
                if (result.HasCard("Trade Route"))
                {
                    result.AdditionalStuff.Add("Trade Route Mat");
                }
                if (result.HasCardType(CardType.Reserve))
                {
                    result.AdditionalStuff.Add("Tavern Mat");
                }
                if (result.HasCard("Embargo"))
                {
                    result.AdditionalStuff.Add("Embargo Tokens");
                }
                if (result.HasCard("Baker") || result.HasCard("Butcher") || result.HasCard("Candlestick Maker") || result.HasCard("Merchant Guild") || result.HasCard("Pirate Ship") || result.HasCard("Plaza") || result.HasCard("Trade Route"))
                {
                    result.AdditionalStuff.Add("Coin Tokens");
                }
                if (result.HasCard(c => !c.IsType(CardType.Victory | CardType.Curse) && c.ContainsText("{VP}")))
                {
                    result.AdditionalStuff.Add("Victory Point Tokens");
                }
                result.AdditionalStuff = result.AdditionalStuff.Distinct().OrderBy(s => s).ToList();
            }
        }

        /// <summary>
        /// Cancel the current generation loop
        /// </summary>
        public static async Task CancelGeneration()
        {
            generationCanceled = true;
            while (isGenerating)
            {
                await Task.Delay(100);
            }
        }
    }
}