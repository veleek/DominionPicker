using Ben.Data;

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
        Adventures,
        Empires,
        Promo,
    }

    public static class CardSetExtensions
    {
        public static string Localize(this CardSet set)
        {
            return Localized.CardData.GetLocalizedValue(set);
        }
    }

}