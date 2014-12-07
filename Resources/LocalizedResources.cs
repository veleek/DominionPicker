using Ben.Dominion.Resources;

namespace Ben.Dominion
{
    public class LocalizedResources
    {
        private static readonly Strings strings = new Strings();

        private static readonly CardDataStrings cardDataStrings = new CardDataStrings();

        public Strings Strings
        {
            get { return strings; }
        }

        public CardDataStrings CardDataStrings
        {
            get { return cardDataStrings; }
        }
    }
}