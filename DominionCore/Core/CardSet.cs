using Ben.Data;

namespace Ben.Dominion
{

    public enum CardSet
    {
        None = 0x0000,
        Base = 0x0001,
        Base2E = 0x0002,
        Intrigue = 0x0004,
        Intrigue2E = 0x0008,
        Seaside = 0x0010,
        Alchemy = 0x0020,
        Prosperity = 0x0040,
        Cornucopia = 0x0080,
        Hinterlands = 0x0100,
        DarkAges = 0x0200,
        Guilds = 0x0400,
        Adventures = 0x0800,
        Empires = 0x1000,
        Promo = 0x2000,
    }

    public static class CardSetExtensions
    {
        public static string Localize(this CardSet set)
        {
            return Localized.CardData.GetLocalizedValue(set);
        }
    }

}