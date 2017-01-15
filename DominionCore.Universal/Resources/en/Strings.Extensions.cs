namespace Ben.Dominion.Resources
{
    using System.Globalization;

    public partial class Strings
    {
        private static CultureInfo culture;

        public static void EnsureLoaded()
        {
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
                if (culture != null)
                {
                    _context.Languages = culture == null ? null : new string[] { culture.ToString() };
                }
            }
        }
    }
}