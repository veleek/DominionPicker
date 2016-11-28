using System;
using System.Text;

namespace DominionPickerTest
{
    public static class RandomExtensions
    {
        public static bool NextBool(this Random random, double probability = 0.5)
        {
            return random.NextDouble() < probability;
        }

        public static TEnum NextEnum<TEnum>(this Random random)
        {
            Array enumValues = Enum.GetValues(typeof(TEnum));
            return (TEnum)enumValues.GetValue(random.Next(enumValues.Length));
        }

        public static string NextString(this Random random, int length, bool includeSpaces = false)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }

            return sb.ToString();
        }
    }
}
