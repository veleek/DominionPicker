using Ben.Data;
using Ben.Data.Converters;

namespace Ben.Dominion.Converters
{
    public class CardDataLocalizeConverter
       : LocalizeConverter
    {

        public CardDataLocalizeConverter()
        : base(Localized.CardData)
        {
        }
    }
}
