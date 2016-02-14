using Ben.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
