using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Ben.Dominion.Resources;

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
        Hinterlands,
        DarkAges,
        Guilds,
        Promo,
    }

    [Flags]
    public enum CardType
    {
        None = 0x0000,
        Treasure = 0x0001,
        Victory = 0x0002,
        Curse = 0x0004,
        Action = 0x0008,
        Reaction = 0x0010,
        Attack = 0x0020,
        Duration = 0x0040,
        Prize = 0x0080,
        Ruins = 0x0100,
        Shelter = 0x0200,
        Looter = 0x0400,
        Knight = 0x0800,
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
        public Boolean HasPotion
        {
            get { return this.Cost.ToLower().Contains('p'); }
        }

        [XmlIgnore]
        public String CoinCost
        {
            get { return this.Cost.Contains('+') ? " " + this.Cost.Replace('+', '\x207A') : this.Cost.Trim('p'); }
        }

        private string setString;

        private string SetString
        {
            get { return this.setString ?? (this.setString = this.Set.ToString()); }
        }

        private string typeString;

        private string TypeString
        {
            get { return this.typeString ?? (this.typeString = this.Type.ToString()); }
        }

        private string displayName;

        [XmlIgnore]
        public string DisplayName
        {
            get { return this.displayName ?? this.Name; }
            set { this.displayName = value; }
        }

        public String SetPrefix
        {
            get { return this.SetString.Substring(0, 4); }
        }

        public String FormattedRules
        {
            get { return (this.Rules ?? "").Replace("\\n", "\n"); }
        }

        public Card()
        {
        }

        public Card(String name, CardType type)
        {
            this.Name = name;
            this.Type = type;
            this.Cost = "0";
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

        public Boolean ContainsText(String filter)
        {
            // Check to see if this card contains each of the words in the filter
            return filter.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).All(this.ContainsWord);
        }

        /// <summary>
        /// Determines if a card contains a specific word in it's name, set, type, or rules
        /// </summary>
        /// <param name="filterWord"></param>
        /// <returns></returns>
        protected Boolean ContainsWord(String filterWord)
        {
            return this.FindSubstring(this.Name, filterWord)
                   || this.FindSubstring(this.SetString, filterWord)
                   || this.FindSubstring(this.TypeString, filterWord)
                   || this.FindSubstring(this.Rules, filterWord)
                ;
        }

        private Boolean FindSubstring(String value, String substring)
        {
            return value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void MergeFrom(Card c, bool mergeRules = true)
        {
            this.DisplayName = c.DisplayName ?? this.DisplayName;

            if (mergeRules)
            {
                this.Rules = c.Rules ?? this.Rules;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} ({2}): {3}", this.Name, this.Type, this.Set, this.Cost);
        }

        public Brush BackgroundColor
        {
            get { return GetBrushForType(this.Type); }
        }

        private static Dictionary<CardType, Brush> brushCache = new Dictionary<CardType, Brush>();

        public static Brush GetBrushForType(CardType type)
        {
            Brush cardTypeBrush = null;

            if (!brushCache.TryGetValue(type, out cardTypeBrush))
            {
                List<Color> colors = GetColorsForType(type);

                if (colors.Count == 1)
                {
                    cardTypeBrush = new SolidColorBrush(colors[0]);
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

                    grad.GradientStops.Add(new GradientStop {Color = first, Offset = 0});
                    grad.GradientStops.Add(new GradientStop {Color = first, Offset = 0.20});

                    grad.GradientStops.Add(new GradientStop {Color = second, Offset = 0.80});
                    grad.GradientStops.Add(new GradientStop {Color = second, Offset = 1.0});

                    cardTypeBrush = grad;
                }

                brushCache[type] = cardTypeBrush;
            }

            return cardTypeBrush;
        }

        public static List<Color> GetColorsForType(CardType type)
        {
            List<Color> colors = new List<Color>();

            for (int i = 0; i < 12; i++)
            {
                CardType typeToCheck = (CardType) (1 << i);

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
                    return Color.FromArgb(255, 235, 180, 15); // #EBB40F
                case CardType.Victory:
                    return Color.FromArgb(255, 97, 121, 57); // #617939
                case CardType.Curse:
                    return Color.FromArgb(255, 123, 65, 141);
                case CardType.Attack:
                    return Color.FromArgb(255, 255, 60, 60);
                case CardType.Shelter:
                    return Color.FromArgb(255, 255, 120, 60);
                case CardType.Reaction:
                    return Color.FromArgb(255, 68, 113, 181);
                case CardType.Duration:
                    return Color.FromArgb(255, 251, 141, 51);
                case CardType.Action:
                case CardType.Knight:
                case CardType.Looter:
                    return Color.FromArgb(255, 160, 155, 165); // #A09BA5
                case CardType.Prize:
                    return Color.FromArgb(255, 153, 217, 234);
                case CardType.Ruins:
                    return Color.FromArgb(244, 188, 128, 16); // #BC8010
                default:
                    return Colors.Black;
            }
        }

        public static Card FromName(String name)
        {
            return Cards.NameLookup[name];
        }

        public static Card FromId(Int32 id)
        {
            return Cards.Lookup[id];
        }
    }

    public class AlphabeticalCardComparer : IComparer<Card>
    {
        public int Compare(Card x, Card y)
        {
            return String.Compare(x.DisplayName, y.DisplayName, CardDataStrings.Culture, CompareOptions.IgnoreCase);
        }
    }
}