using Ben.Data;
using Ben.Data.Converters;

namespace Ben.Dominion.Converters
{
    public class StringsLocalizeConverter
       : LocalizeConverter
    {

        public StringsLocalizeConverter()
        : base(Localized.Strings)
        {
        }
    }
}
