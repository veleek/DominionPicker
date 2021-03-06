namespace Ben.Dominion.Resources
{
    /// <summary>
    /// A helper class used by XAML for binding to various localized strings.
    /// </summary>
    public class LocalizedResources
    {
        private static readonly Strings strings = new Strings();
        private static readonly CardDataStrings cardDataStrings = new CardDataStrings();

        public Strings Strings
        {
            get
            {
                return strings;
            }
        }

        public CardDataStrings CardDataStrings
        {
            get
            {
                return cardDataStrings;
            }
        }
    }
}