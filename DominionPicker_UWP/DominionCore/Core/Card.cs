using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using Ben.Dominion.Resources;
using Ben.Dominion.ViewModels;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Ben.Dominion
{

    public class Card
    {
        private string setString;
        private string typeString;
        private string displayName;

        public Card()
        {
        }

        public Card(String name, CardType type)
        {
            this.Name = name;
            this.Type = type;
        }

        [XmlAttribute]
        public Int32 ID { get; set; }

        [XmlAttribute]
        public String Name { get; set; }

        [XmlAttribute]
        public CardSet Set { get; set; }

        [XmlAttribute]
        public CardType Type { get; set; }

        [XmlAttribute]
        public String Cost { get; set; } = "0";

        [XmlAttribute]
        public String Rules { get; set; }

        /// <summary>
        /// Gets or sets a property that indicates whether or not this card
        /// can be selected during the Kingdom card selection process.
        /// </summary>
        /// <remarks>
        /// This is generally used to indicate 'secondary' cards that are included
        /// automatically by another card.  For example, 'Knights' is used as a 
        /// randomizer card and each of the individual Knights is not pickable, so
        /// they will not be used by the randomizer.
        /// </remarks>
        [XmlAttribute]
        public Boolean Pickable { get; set; } = true;

        [XmlIgnore]
        public string Label { get; set; }

        [XmlIgnore]
        public CardGroup Group { get; set; }

        [XmlIgnore]
        public Boolean HasPotion
        {
            get
            {
                return this.Cost.ToLower().Contains('p');
            }
        }

        [XmlIgnore]
        public String CoinCost
        {
            get
            {
                // Remove the P from the cost;
                string coinCost = this.Cost.Trim('p').ToLower();
                if (coinCost.Contains('+'))
                {
                    // Replace the + (e.g. 3+) with a superscript plus character
                    coinCost = " " + this.Cost.Replace('+', '\x207A');
                }
                return coinCost;
            }
        }

        [XmlIgnore]
        public string StrategyPageUrl
        {
            get
            {
                return string.Format("http://wiki.dominionstrategy.com/index.php/{0}", this.Name.Replace(" ", "_"));
            }
        }

        private string SetString
        {
            get
            {
                return this.setString ?? (this.setString = this.Set.ToString());
            }
        }

        private string TypeString
        {
            get
            {
                return this.typeString ?? (this.typeString = this.Type.ToString());
            }
        }

        [XmlIgnore]
        public string DisplayName
        {
            get
            {
                return this.displayName ?? this.Name;
            }
            set
            {
                this.displayName = value;
            }
        }

        public String SetPrefix
        {
            get
            {
                return this.SetString.Substring(0, 4);
            }
        }

        public String FormattedRules
        {
            get
            {
                return (this.Rules ?? "").Replace("\\n", "\n");
            }
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
            return filter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).All(this.ContainsWord);
        }

        /// <summary>
        /// Determines if a card contains a specific word in it's name, set, type, or rules
        /// </summary>
        /// <param name="filterWord"></param>
        /// <returns></returns>
        protected Boolean ContainsWord(String filterWord)
        {
            return this.FindSubstring(this.Name, filterWord) || this.FindSubstring(this.DisplayName, filterWord) || this.FindSubstring(this.SetString, filterWord) || this.FindSubstring(this.TypeString, filterWord) || (this.Rules != null && this.FindSubstring(this.Rules, filterWord));
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

        public Card Clone(string label = null)
        {
            return this.MemberwiseClone() as Card;
        }

        public Card WithLabel(string label)
        {
            if (label == null)
            {
                throw new ArgumentNullException("label");
            }
            Card clone = this.Clone();
            clone.Label = label;
            return clone;
        }

        public Card WithGroup(CardGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            Card clone = this.Clone();
            clone.Group = group;
            return clone;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} ({2}): {3}", this.Name, this.Type, this.Set, this.Cost);
        }

        [XmlIgnore]
        public Brush BackgroundColor
        {
            get
            {
                return GetBrushForType(this.Type);
            }
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
                    grad.StartPoint = new Windows.Foundation.Point(0.5, 0);
                    grad.EndPoint = new Windows.Foundation.Point(0.5, 1);
                    //Double offsetStep = 1.0 / (colors.Count - 1);
                    //Double offset = 0.0;
                    if (colors.Count > 2)
                    {
                        colors.Remove(Color.FromArgb(255, 160, 155, 165));
                    }
                    Color first = colors[0];
                    Color second = colors[1];
                    grad.GradientStops.Add(new GradientStop
                    {
                        Color = first,
                        Offset = 0
                    });
                    grad.GradientStops.Add(new GradientStop
                    {
                        Color = first,
                        Offset = 0.20
                    });
                    grad.GradientStops.Add(new GradientStop
                    {
                        Color = second,
                        Offset = 0.80
                    });
                    grad.GradientStops.Add(new GradientStop
                    {
                        Color = second,
                        Offset = 1.0
                    });
                    cardTypeBrush = grad;
                }
                brushCache[type] = cardTypeBrush;
            }
            return cardTypeBrush;
        }

        public static List<Color> GetColorsForType(CardType type)
        {
            List<Color> colors = new List<Color>();
            CardType typeToCheck = (CardType)(1);
            while (Enum.IsDefined(typeof(CardType), typeToCheck))
            {
                switch (typeToCheck)
                {
                    case CardType.Treasure:
                    case CardType.Victory:
                    case CardType.Curse:
                    case CardType.Action:
                    case CardType.Reaction:
                    case CardType.Attack:
                    case CardType.Duration:
                    case CardType.Ruins:
                    case CardType.Reserve:
                    case CardType.Event:
                    case CardType.Shelter:
                    case CardType.Prize:
                        if (type.HasFlag(typeToCheck))
                        {
                            colors.Add(GetColorForType(typeToCheck));
                        }
                        break;
                }
                typeToCheck = (CardType)((int)typeToCheck << 1);
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
                case CardType.Traveller:
                case CardType.Event:
                    return Color.FromArgb(255, 160, 155, 165); // #A09BA5

                case CardType.Prize:
                    return Color.FromArgb(255, 71, 135, 255); // #4787FF

                case CardType.Ruins:
                    return Color.FromArgb(255, 188, 128, 16); // #BC8010

                case CardType.Reserve:
                    return Color.FromArgb(255, 190, 165, 100);
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

    public class AlphabeticalCardComparer
       : IComparer<Card>
    {

        public int Compare(Card x, Card y)
        {
            CompareInfo c = CompareInfo.GetCompareInfo(CardDataStrings.Culture.ToString());
            return c.Compare(x.DisplayName, y.DisplayName, CompareOptions.IgnoreCase);
        }

    }

    public class CardIdComparer
       : EqualityComparer<Card>
    {
        private static CardIdComparer defaultComparer;

        public new static CardIdComparer Default
        {
            get
            {
                return defaultComparer ?? (defaultComparer = new CardIdComparer());
            }
        }

        public override bool Equals(Card x, Card y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null ^ y == null)
            {
                return false;
            }
            return x.ID == y.ID;
        }

        public override int GetHashCode(Card card)
        {
            return card.ID.GetHashCode();
        }

    }

}