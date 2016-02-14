using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using Ben.Data;
using Ben.Dominion.Models;
using Ben.Dominion.Resources;
using Ben.Utilities;
using Microsoft.Xna.Framework;

namespace Ben.Dominion
{
    public static class Cards
    {
        public static readonly string PickerCardsFileName = "./Resources/DominionPickerCards.xml";

        private static ReadOnlyCollection<Card> allCards;
		private static ReadOnlyCollection<CardSet> allSets;
	    private static ReadOnlyCollection<CardType> allTypes;
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
				    allTypes = new ReadOnlyCollection<CardType>(
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
			    }

			    return allTypes;
		    }
        }

        public static ReadOnlyCollection<Card> AllCards
        {
            get
            {
                if (allCards == null)
                {
                    allCards = new ReadOnlyCollection<Card>(Load().OrderBy(c => c.Set).ThenBy(c => c.Name).ToList());
                }
                return allCards;
            }
        }

        public static ReadOnlyCollection<String> NonPickableCards
        {
            get
            {
                return new ReadOnlyCollection<string>(
                    new List<string>
                    {
                        // Base Cards
                        "Copper",
                        "Silver",
                        "Gold",
                        "Estate",
                        "Duchy",
                        "Province",
                        "Curse",

                        // Alchemy Treasures
                        "Potion",

                        // Prosperity Treasures and Victorys
                        "Platinum",
                        "Colony",

                        // Prizes
                        "Bag of Gold",
                        "Diadem",
                        "Followers",
                        "Princess",
                        "Trusty Steed",
                        "Prizes",

                        // Dark Ages, non-Supply Cards
                        "Spoils",
                        "Madman",
                        "Mercenary",

                        // Shelters
                        "Shelters",
                        "Hovel",
                        "Necropolis",
                        "Overgrown Estate",

                        // Ruins
                        "Ruins",
                        "Abandoned Mine",
                        "Ruined Library",
                        "Ruined Market",
                        "Ruined Village",
                        "Survivors",

                        // Knights
                        "Dame Anna",
                        "Dame Josephine",
                        "Dame Molly",
                        "Dame Natalie",
                        "Dame Sylvia",
                        "Sir Bailey",
                        "Sir Destry",
                        "Sir Martin",
                        "Sir Michael",
                        "Sir Vander",

                        // Traveller Upgrade Cards
                        "Champion",
                        "Disciple",
                        "Fugitive",
                        "Hero",
                        "Soldier",
                        "Teacher",
                        "Treasure Hunter",
                        "Warrior",

                        // Events
                        "Alms",
                        "Borrow",
                        "Quest",
                        "Save",
                        "Scouting Party",
                        "Travelling Fair",
                        "Bonfire",
                        "Expedition",
                        "Ferry",
                        "Plan",
                        "Mission",
                        "Pilgrimage",
                        "Ball",
                        "Raid",
                        "Seaway",
                        "Trade",
                        "Lost Arts",
                        "Training",
                        "Inheritance",
                        "Pathfinding",
                        "Summon",
                    }
                    );
            }
        }

        public static ReadOnlyCollection<Card> PickableCards
        {
            get
            {
                if (pickableCards == null)
                {
                    pickableCards = new ReadOnlyCollection<Card>(
                        AllCards.Where(c => !NonPickableCards.Contains(c.Name)).ToList()
                    );
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

        public static List<Card> Load()
        {
            List<Card> cards;

            using (var stream = TitleContainer.OpenStream(PickerCardsFileName))
            {
                cards = GenericXmlSerializer.Deserialize<List<Card>>(stream);
            }

            string localizedFileName = CardDataStrings.Application_LocalizedCardsFileName;
            if (!string.IsNullOrWhiteSpace(localizedFileName))
            {
                List<Card> localizedCards;
                using (var stream = TitleContainer.OpenStream(localizedFileName))
                {
                    localizedCards = GenericXmlSerializer.Deserialize<List<Card>>(stream);
                }

                if (localizedCards != null && localizedCards.Count > 0)
                {
                    var localizeLookup = localizedCards.ToDictionary(c => c.ID);

                    bool mergeRulesText = ConfigurationModel.Instance.LocalizeRulesText;

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
    }

    /// <summary>
    /// A grouping of card selectors by set
    /// </summary>
    public class CardSetGrouping : ObservableGrouping<CardSet, CardSelector>
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
    }

    /// <summary>
    /// A simple key value pair to joins a specific card with a filtered status
    /// that can be used for data binding a check box.
    /// </summary>
    public class CardSelector : IComparable<CardSelector>
    {
        public CardSelector()
        {
        }

        public CardSelector(Card card, bool selected)
        {
            this.Card = card;
            this.Selected = selected;
        }

        public Card Card { get; set; }
        public bool Selected { get; set; }
        public bool Filtered { get; set; }

        public int CompareTo(CardSelector other)
        {
            return String.CompareOrdinal(this.Card.Name, other.Card.Name);
        }

        public bool Filter(bool filtered)
        {
            if (filtered == this.Filtered)
            {
                return false;
            }

            this.Filtered = filtered;
            return true;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0}] {1}", this.Selected ? "X" : "_", this.Card);
        }
    }
}