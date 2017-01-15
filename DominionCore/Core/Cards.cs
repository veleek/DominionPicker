using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ben.Dominion.Resources;
using Ben.Utilities;
using System.Reflection;

namespace Ben.Dominion
{
    public static class Cards
    {
        public static readonly string PickerCardsFileName = @".\Resources\DominionPickerCards.xml";

        public static ReadOnlyCollection<CardSet> AllSets { get; } = 
            new ReadOnlyCollection<CardSet>(
                new List<CardSet>
                {
                    CardSet.Base,
                    CardSet.Base2E,
                    CardSet.Intrigue,
                    CardSet.Intrigue2E,
                    CardSet.Seaside,
                    CardSet.Alchemy,
                    CardSet.Prosperity,
                    CardSet.Cornucopia,
                    CardSet.Hinterlands,
                    CardSet.DarkAges,
                    CardSet.Guilds,
                    CardSet.Adventures,
                    CardSet.Empires,
                    CardSet.Promo,
                }
            );

        public static ReadOnlyCollection<CardType> AllTypes { get; } =
            new ReadOnlyCollection<CardType>(
                new List<CardType>
                {
                    CardType.Action,
                    CardType.Attack,
                    CardType.Curse,
                    CardType.Duration,
                    CardType.Knight,
                    CardType.Looter,
                    CardType.Prize,
                    CardType.Reaction,
                    CardType.Ruins,
                    CardType.Shelter,
                    CardType.Treasure,
                    CardType.Victory,
                }
            );

        public static ReadOnlyCollection<Card> AllCards { get; private set; }
        
        public static ReadOnlyCollection<Card> NonPickableCards { get; private set; }

        public static ReadOnlyCollection<Card> PickableCards { get; private set; }

        public static Dictionary<Int32, Card> Lookup { get; private set; }

        public static Dictionary<string, Card> NameLookup { get; private set; }

        /// <summary>
        /// Force everything to get loaded and initialize all of the sets.
        /// </summary>
        /// <returns></returns>
        public static async Task EnsureLoaded()
        {
            if(AllCards == null)
            {
                var cards = (await Load().ConfigureAwait(false)).OrderBy(c => c.Set).ThenBy(c => c.Name).ToList();
                AllCards = new ReadOnlyCollection<Card>(cards);
                NonPickableCards = new ReadOnlyCollection<Card>(cards.Where(c => !c.Pickable).ToList());
                PickableCards = new ReadOnlyCollection<Card>(cards.Where(c => c.Pickable).ToList());
                Lookup = cards.ToDictionary(c => c.ID);
                NameLookup = cards.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            }
        }

        public static async Task<List<Card>> Load(bool localizeRulesText = false)
        {
            // Load the base list of cards.
            List<Card> cards = await LoadCardsFromFileAsync(PickerCardsFileName).ConfigureAwait(false);

            // Check to see if we should load any localized cards.  This string is localized
            // so that we find the appropriate file depending on our locale.
            string localizedFileName = CardDataStrings.Application_LocalizedCardsFileName;
            if (!string.IsNullOrWhiteSpace(localizedFileName))
            {
                List<Card> localizedCards = await LoadCardsFromFileAsync(localizedFileName).ConfigureAwait(false);
                if (localizedCards != null && localizedCards.Count > 0)
                {
                    var localizeLookup = localizedCards.ToDictionary(c => c.ID);
                    // Loop through each card in the current set, and merge the localized
                    // version ontop if we have it.
                    foreach (var card in cards)
                    {
                        Card c;
                        if (localizeLookup.TryGetValue(card.ID, out c))
                        {
                            card.MergeFrom(c, mergeRules: localizeRulesText);
                        }
                    }
                }
            }

            return cards;
        }

        private static async Task<List<Card>> LoadCardsFromFileAsync(string fileName)
        {
#if NETFX_CORE
            // On NETFX_CORE we need to append the assembly name to get deployed resources
            fileName = fileName.TrimStart('.', '\\', '/');
            fileName = System.IO.Path.Combine(typeof(Cards).GetTypeInfo().Assembly.GetName().Name, fileName);
#endif
            using (var stream = await FileUtility.OpenApplicationStreamAsync(fileName).ConfigureAwait(false))
            {
                return GenericXmlSerializer.Deserialize<List<Card>>(stream);
            }
        }
    }
}