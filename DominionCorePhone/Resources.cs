using System.Globalization;
using System.Resources;

namespace Ben.Dominion.Resources
{
    public partial class Strings
    {
        public static ResourceManager ResourceManager { get; set; }

        public static CultureInfo Culture { get; set; }
    }

    public partial class CardDataStrings
    {
        public static ResourceManager ResourceManager { get; set; }

        public static CultureInfo Culture { get; set; }
        public static string Application_LocalizedCardsFileName { get; internal set; }
    }
}
