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
        public Boolean InSet(CardSet set)
        {
            return set == this.Set;
        }

        public Boolean InSet(IEnumerable<CardSet> sets)
        {
            return sets.Contains(this.Set);
        }

        [XmlAttribute]
        public String Name { get; set; }
        [XmlAttribute]
        public CardSet Set { get; set; }
        [XmlAttribute]
        public CardType Type { get; set; }
        [XmlAttribute]
        public String Cost { get; set; }

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
                    Double offsetStep = 1.0 / (colors.Count - 1);
                    Double offset = 0.0;

                    foreach (Color c in colors)
                    {
                        GradientStop stop = new GradientStop();
                        stop.Color = c;
                        stop.Offset = offset;
                        grad.GradientStops.Add(stop);

                        offset += offsetStep;
                    }

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
                    return Colors.Yellow;
                case CardType.Victory:
                    return Colors.Green;
                case CardType.Curse:
                    return Colors.Purple;
                case CardType.Attack:
                    return Colors.Red;
                case CardType.Reaction:
                    return Colors.Blue;
                case CardType.Duration:
                    return Colors.Orange;
                case CardType.Action:
                    return Colors.Gray;
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

            /*
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var x = Application.GetResourceStream(new Uri(PickerCardsFileName, UriKind.Relative)).Stream;
                var y = (new StreamReader(x)).ReadToEnd();


                if (store.FileExists(PickerCardsFileName))
                {
                    using (IsolatedStorageFileStream stream = store.OpenFile(PickerCardsFileName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        cards = GenericSerializer.Deserialize<List<Card>>(stream);
                    }
                }
                else
                {
                    cards = new List<Card>();
                }
            }
             */

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