using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Data;
using Ben.Data;
using Ben.Utilities;

namespace Ben.Dominion
{
    public static class Cards
    {
        public static readonly String PickerCardsFileName = "./Assets/DominionPickerCards.xml";

        public static IEnumerable<CardSet> AllSets
        {
            get
            {
                return new List<CardSet>
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
                    CardSet.Promo,
                };
            }
        }

        public static IEnumerable<CardType> AllTypes
        {
            get
            {
                return new List<CardType>
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
                };
            }
        }

        private static Dictionary<CardSet, List<Card>> cardsBySet = new Dictionary<CardSet, List<Card>>();
        public static List<Card> GetSet(CardSet set)
        {
            if (!cardsBySet.ContainsKey(set))
            {
                cardsBySet[set] = AllCards.Where(c => c.Set == set).ToList();
            }

            return cardsBySet[set];
        }

        private static ReadOnlyCollection<Card> allCards = null;
        public static ReadOnlyCollection<Card> AllCards 
        {
            get
            {
                if (allCards == null)
                {
                    allCards = new ReadOnlyCollection<Card>(Load().OrderBy(c => c.SetPrefix + c.Name).ToList());
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
                    }
                );
            }
        }

        private static ReadOnlyCollection<Card> pickableCards = null;
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

        private static Dictionary<Int32, Card> lookup = null;
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

        private static Dictionary<string, Card> nameLookup = null;
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
            using (var stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(PickerCardsFileName))
            {
                return GenericXmlSerializer.Deserialize<List<Card>>(stream);
            }
        }

        public static void Save()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.CreateFile(PickerCardsFileName))
                {
                    String xml = GenericXmlSerializer.Serialize(AllCards);
                    Console.WriteLine(xml);
                }
            }
        }
    }

    /// <summary>
    /// A grouping of card selectors by set
    /// </summary>
    public class CardGrouping : ObservableGrouping<CardSet, CardSelector>
    {
        public CardGrouping(CardSet key, IEnumerable<CardSelector> cards)
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
        public CardSelector() { }
        public CardSelector(Card card, bool selected)
        {
            this.Card = card;
            this.Selected = selected;
        }

        public Card Card { get; set; }
        public bool Selected { get; set; }
        public bool Filtered { get; set; }

        public bool Filter(bool filtered)
        {
            if (filtered == this.Filtered)
            {
                return false;
            }

            this.Filtered = filtered;
            return true;
        }

        public int CompareTo(CardSelector other)
        {
            return String.CompareOrdinal(this.Card.Name, other.Card.Name);
            //return this.Card.ID.CompareTo(other.Card.ID);
        }
    }

    public class CardNameComparer : Comparer<Card>, IEqualityComparer<Card>
    {
        public override int Compare(Card x, Card y)
        {
            return x.Name.CompareTo(y.Name);
        }

        public bool Equals(Card x, Card y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Card card)
        {
            return card.Name.GetHashCode();
        }
    }
}