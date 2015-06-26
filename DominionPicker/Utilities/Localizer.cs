using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Reflection;
using Ben.Dominion.Resources;
using Ben.Dominion.ViewModels;
using System.Linq;

namespace Ben.Data
{
    public class Localizer
    {
        private static Dictionary<object, string> localizedValueMap = new Dictionary<object, string>();

        public ResourceManager ResourceManager { get; set; }

        public CultureInfo DefaultCulture { get; set; }

        public string GetLocalizedValue(object value, params object[] args)
        {
            return GetLocalizedValue(value, DefaultCulture, args);
        }

        public string GetLocalizedValue(object value, CultureInfo culture, params object[] args)
        {
            if (value == null)
            {
                return "<!! NULL !!>";
            }

            ILocalizable localizableValue = value as ILocalizable;
            string localizedValue;
            if (!localizedValueMap.TryGetValue(value, out localizedValue))
            {
                string valueType = value.GetType().Name;
                string valueName = value.ToString();
                string localizedValueKey = string.Format("{0}_{1}", valueType, valueName);

                Enum enumValue = value as Enum;
                bool isFlags = enumValue != null && enumValue.GetType().GetCustomAttributes<FlagsAttribute>().Any();
                if (isFlags && ((int)value) != 0)
                {
                    List<string> localizedEnumParts = new List<string>();
                    foreach (var flag in Enum.GetValues(enumValue.GetType()))
                    {
                        if (((int)flag != 0) && enumValue.HasFlag((Enum)flag))
                        {
                            localizedEnumParts.Add(GetRawLocalizedValue(flag, culture));
                        }
                    }

                    localizedValue = string.Join(", ", localizedEnumParts);
                }
                else if(localizableValue != null)
                {
                    // If it's localized, then we'll let them determine
                    // the appropriate way to get the raw localized value
                    localizedValue = localizableValue.ToString(this);
                }
                else
                {
                    localizedValue = ResourceManager.GetString(localizedValueKey, culture ?? DefaultCulture ?? CultureInfo.InvariantCulture) ?? "!! Missing Resource for " + localizedValueKey + " !!";
                }

                localizedValueMap[value] = localizedValue;
            }

            if (localizableValue != null)
            {
                args = args == null || args.Length == 0 ? localizableValue.LocalizedContext : args;
            }

            if (args != null && args.Length > 0)
            {
                return string.Format(localizedValue, args);
            }

            return localizedValue;
        }
        
        private string GetRawLocalizedValue(object value, CultureInfo culture)
        {
            string localizedValue;
            if (!localizedValueMap.TryGetValue(value, out localizedValue))
            {
                string valueType = value.GetType().Name;
                string valueName = value.ToString();
                string localizedValueKey = string.Format("{0}_{1}", valueType, valueName);

                localizedValue = ResourceManager.GetString(localizedValueKey, culture ?? DefaultCulture ?? CultureInfo.InvariantCulture) ?? "!! Missing Resource !!";
                localizedValueMap[localizedValueKey] = localizedValue;
            }

            return localizedValue;
        }
    }

    public static class Localized
    {
        public static Localizer Strings { get; } = new Localizer
        {
            ResourceManager = Ben.Dominion.Resources.Strings.ResourceManager,
            DefaultCulture = Ben.Dominion.Resources.Strings.Culture,
        };

        public static Localizer CardData { get; } = new Localizer
        {
            ResourceManager = CardDataStrings.ResourceManager,
            DefaultCulture = CardDataStrings.Culture,
        };
    }
}
