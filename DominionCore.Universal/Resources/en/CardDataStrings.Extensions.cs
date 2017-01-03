namespace Ben.Dominion.Resources
{
    using System.Globalization;
    using Windows.ApplicationModel.Resources.Core;

    public partial class CardDataStrings
    {
        private static CultureInfo culture;
        private static readonly ResourceContext context = new ResourceContext();
        private static readonly ResourceMap map = ResourceManager.Current.MainResourceMap.GetSubtree("DominionCore.Universal/CardDataStrings");

        public static void EnsureLoaded()
        {
            // This method does nothing except make sure that 
        }

        public static string GetString(string key)
        {
            if(Culture != null)
            {
                ResourceCandidate localizedCandidate = map.GetValue(key, context);
                return localizedCandidate.ValueAsString;
            }

            return resourceLoader.GetString(key);
        }

        public static CultureInfo Culture
        {
            get { return culture; }
            set
            {
                culture = value;
                context.Languages = culture == null ? null : new string[] { culture.ToString() };
            }
        }
    }
}