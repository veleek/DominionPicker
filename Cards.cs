using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Resources;
using Ben.Utilities;

namespace Ben.Dominion
{
    public enum CardSet
    {
        None,
        Base,
        Intrigue,
        Seaside,
        Alchemy,
        Prosperity,
        Cornucopia,
        Promo,
    }

    [Flags]
    public enum CardType
    {
        None = 0x00,
        Treasure = 0x01,
        Victory = 0x02,
        Curse = 0x04,
        Action = 0x08,
        Reaction = 0x10,
        Attack = 0x20,
        Duration = 0x40,
        Prize = 0x80,
    }

    public class Card
    {
        [XmlAttribute]
        public Int32 ID { get; set; }
        [XmlAttribute]
        public String Name { get; set; }
        [XmlAttribute]
        public CardSet Set { get; set; }
        [XmlAttribute]
        public CardType Type { get; set; }
        [XmlAttribute]
        public String Cost { get; set; }
        [XmlAttribute]
        public String Rules { get; set; }

        [XmlIgnore]
        public Boolean HasPotion { get { return Cost.Contains('p'); } }
        [XmlIgnore]
        public String CoinCost { get { return Cost.Trim('p'); } }

        public String SetPrefix
        {
            get { return Set.ToString().Substring(0, 4); }
        }

        public String FormattedRules
        {
            get { return Rules.Replace("\\n", "\n"); }
        }

        public Card() { }

        public Card(String name, CardType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public Boolean InSet(CardSet set)
        {
            return set == this.Set;
        }

        public Boolean InSet(IEnumerable<CardSet> sets)
        {
            return sets.Contains(this.Set);
        }

        public Boolean IsType(CardType cardType)
        {
            return (this.Type & cardType) != CardType.None;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} ({2}): {3}", Name, Type, Set, Cost);
        }
        
        public Brush BackgroundColor
        {
            get
            {
                List<Color> colors = GetColorsForType(Type);

                if (colors.Count == 1)
                {
                    return new SolidColorBrush(colors[0]);
                }
                else
                {
                    LinearGradientBrush grad = new LinearGradientBrush();
                    grad.StartPoint = new Point(0.5, 0);
                    grad.EndPoint = new Point(0.5, 1);
                    //Double offsetStep = 1.0 / (colors.Count - 1);
                    //Double offset = 0.0;

                    Color first = colors[0];
                    Color second = colors[1];

                    grad.GradientStops.Add(new GradientStop { Color = first, Offset = 0 });
                    grad.GradientStops.Add(new GradientStop { Color = first, Offset = 0.20 });

                    grad.GradientStops.Add(new GradientStop { Color = second, Offset = 0.80 });
                    grad.GradientStops.Add(new GradientStop { Color = second, Offset = 1.0 });

                    return grad;

                }
            }
        }

        public static List<Color> GetColorsForType(CardType type)
        {
            List<Color> colors = new List<Color>();

            for (int i = 0; i < 7; i++)
            {
                CardType typeToCheck = (CardType)(1 << i);

                if ((type & typeToCheck) != CardType.None)
                {
                    colors.Add(GetColorForType(typeToCheck));
                }
            }

            return colors;
        }

        public static Color GetColorForType(CardType type)
        {
            switch (type)
            {
                case CardType.Treasure:
                    return Color.FromArgb(255, 235, 180, 15);
                case CardType.Victory:
                    return Color.FromArgb(255, 97, 121, 57);
                case CardType.Curse:
                    return Color.FromArgb(255, 123, 65, 141);
                case CardType.Attack:
                    return Color.FromArgb(255, 255, 60, 60);
                case CardType.Reaction:
                    return Color.FromArgb(255, 68, 113, 181);
                case CardType.Duration:
                    return Color.FromArgb(255, 251, 141, 78);
                case CardType.Action:
                    return Color.FromArgb(255, 160, 155, 165);
                case CardType.Prize:
                    return Color.FromArgb(255, 153, 217, 234);
                default:
                    return Colors.Black;
            }
        }
    }

    public class Cards
    {
        public static readonly String PickerCardsFileName = "./DominionPickerCards.xml";

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
                    CardType.Treasure,
                    CardType.Victory,
                    CardType.Curse,
                    CardType.Action,
                    CardType.Reaction,
                    CardType.Attack,
                    CardType.Duration,
                    CardType.Prize,
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

        private static List<Card> allCards = null;
        public static List<Card> AllCards 
        {
            get
            {
                if (allCards == null)
                {
                    allCards = Load();
                }
                return allCards;
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
            List<Card> cards = AllCards;
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.CreateFile(PickerCardsFileName))
                {
                    String xml = GenericXmlSerializer.Serialize(cards);
                }
            }
        }
    }
}