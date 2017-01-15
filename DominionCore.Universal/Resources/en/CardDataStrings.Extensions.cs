namespace Ben.Dominion.Resources
{
    using System.Globalization;
    using Windows.ApplicationModel.Resources.Core;

    public partial class CardDataStrings
    {
        private static CultureInfo culture;

        public static void EnsureLoaded()
        {
            // This method does nothing except make sure that this class is properly loaded ahead of time.
        }

        public static CultureInfo Culture
        {
            get
            {
                return culture;
            }

            set
            {
                culture = value;
                _context.Languages = culture == null ? null : new string[] { culture.ToString() };
            }
        }
    }
}