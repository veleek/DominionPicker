using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Resources;

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
        public Int32 Cards { get; set; }
        [XmlIgnore]
        public Int32 Actions { get; set; }
        [XmlIgnore]
        public Int32 Buys { get; set; }
        [XmlIgnore]
        public Int32 Treasure { get; set; }

        public Boolean PlusAction { get { return Actions > 0; } }
        public Boolean PlusBuy { get { return Buys > 0; } }

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
                default:
                    return Colors.Black;
            }
        }
    }

    public class Cards
    {
        public static readonly String PickerCardsFileName = "/DominionPickerCards.xml";

        public static IEnumerable<CardSet> AllSets { get { return Enumerable.Range(1, 5).Select(x => (CardSet)x); } }
        public static IEnumerable<CardType> AllTypes { get { return Enumerable.Range(1, 7).Select(x => (CardType)x); } }

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
            List<Card> cards = null;

            Uri cardsUri = new Uri("/DominionPicker;component/DominionPickerCards.xml", UriKind.Relative);
            StreamResourceInfo resInfo = Application.GetResourceStream(cardsUri);
            if (resInfo != null)
            {
                cards = GenericSerializer.Deserialize<List<Card>>(resInfo.Stream);
            }

            return cards;
        }

        public static void Save()
        {
            List<Card> cards = AllCards;
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.CreateFile(PickerCardsFileName))
                {
                    String xml = GenericSerializer.Serialize(cards);
                }
            }
        }
    }
}