using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ben.Data;
using Ben.Dominion.Resources;
using Ben.Utilities;
using Windows.ApplicationModel;

namespace Ben.Dominion
{

    public static class Cards
    {
        public static readonly string PickerCardsFileName = @"DominionCore\Resources\DominionPickerCards.xml";
        public static ReadOnlyCollection<Card> allCards;
        private static ReadOnlyCollection<CardSet> allSets;
        private static ReadOnlyCollection<CardType> allTypes;
        private static ReadOnlyCollection<Card> nonPickableCards;
        private static ReadOnlyCollection<Card> pickableCards;
        private static Dictionary<Int32, Card> lookup;
        private static Dictionary<string, Card> nameLookup;

        public static ReadOnlyCollection<CardSet> AllSets
        {
            get
            {
                if (allSets == null)
                {
                    allSets = new ReadOnlyCollection<CardSet>(
                        new List<CardSet>
                        {
                            CardSet.Base,
                            CardSet.Intrigue,
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
                }
                return allSets;
            }
        }

        public static ReadOnlyCollection<CardType> AllTypes
        {
            get
            {
                if (allTypes == null)
                {
                    allTypes = new ReadOnlyCollection<CardType>(new List<CardType>
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
                }
                return allTypes;
            }
        }

        public static ReadOnlyCollection<Card> AllCards
        {
            get
            {
                try
                {
                    if (allCards == null)
                    {
                        allCards = new ReadOnlyCollection<Card>((Load().Result).OrderBy(c => c.Set).ThenBy(c => c.Name).ToList());
                    }
                    return allCards;
                }
                catch (AggregateException ae)
                {
                    Exception ie = ae.InnerException;
                    string em = ie.ToString();
                    System.Diagnostics.Debug.WriteLine(ie.ToString());
                    throw;
                }
            }
        }

        public static ReadOnlyCollection<Card> NonPickableCards
        {
            get
            {
                if(nonPickableCards == null)
                {
                    nonPickableCards = new ReadOnlyCollection<Card>(AllCards.Where(c => !c.Pickable).ToList());
                }
                return nonPickableCards;
            }
        }

        public static ReadOnlyCollection<Card> PickableCards
        {
            get
            {
                if (pickableCards == null)
                {
                    pickableCards = new ReadOnlyCollection<Card>(AllCards.Where(c => c.Pickable).ToList());
                }
                return pickableCards;
            }
        }

        public static Dictionary<Int32, Card> Lookup
        {
            get
            {
                if (lookup == null)
                {
                    lookup = AllCards.ToDictionary(c => c.ID);
                }
                return lookup;
            }
        }

        public static Dictionary<string, Card> NameLookup
        {
            get
            {
                if (nameLookup == null)
                {
                    nameLookup = AllCards.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
                }
                return nameLookup;
            }
        }

        public static async Task<List<Card>> Load(bool localizeRulesText = false)
        {
            // Load the base list of cards.
            List<Card> cards = await LoadCardsFromFileAsync(PickerCardsFileName);

            // Check to see if we should load any localized cards.  This string is localized
            // so that we find the appropriate file depending on our locale.
            string localizedFileName = CardDataStrings.Application_LocalizedCardsFileName;
            if (!string.IsNullOrWhiteSpace(localizedFileName))
            {
                List<Card> localizedCards = await LoadCardsFromFileAsync(localizedFileName);
                if (localizedCards != null && localizedCards.Count > 0)
                {
                    var localizeLookup = localizedCards.ToDictionary(c => c.ID);
                    bool mergeRulesText = localizeRulesText;
                    // Loop through each card in the current set, and merge the localized
                    // version ontop if we have it.
                    foreach (var card in cards)
                    {
                        Card c;
                        if (localizeLookup.TryGetValue(card.ID, out c))
                        {
                            card.MergeFrom(c, mergeRulesText);
                        }
                    }
                }
            }

            return cards;
        }

        private static async Task<List<Card>> LoadCardsFromFileAsync(string fileName)
        {
            using (var stream = await FileUtility.OpenApplicationStreamAsync(fileName))
            {
                return GenericXmlSerializer.Deserialize<List<Card>>(stream);
            }
        }
    }

    /// <summary>
    /// A grouping of card selectors by set
    /// </summary>
    public class CardSetGrouping
       : ObservableGrouping<CardSet, CardSelector>
    {
        public CardSetGrouping(CardSet key, IEnumerable<CardSelector> cards)
        : base(key, cards)
        {
        }

        public void SortedInsert(CardSelector card)
        {
            int i;
            for (i = 0; i < this.Count; i++)
            {
                if (card.CompareTo(this[i]) < 0)
                {
                    break;
                }
            }
            this.Insert(i, card);
        }

        public override string ToString()
        {
            return "CardSetGrouping: " + this.Key;
        }
    }
}