namespace Ben.Dominion.Resources
{
    using System.Globalization;

    public partial class Strings
    {
        public static string GetString(string key)
        {
            return resourceLoader.GetString(key);
        }

        public static CultureInfo Culture { get; set; }
    }

}