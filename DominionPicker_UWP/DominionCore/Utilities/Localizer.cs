using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Ben.Dominion.Resources;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;

namespace Ben.Data
{

    public class Localizer
    {
        private static Dictionary<object, string> localizedValueMap = new Dictionary<object, string>();
        private readonly string resourceSubTree;
        private readonly Func<ResourceLoader> resourceManagerGetter;
        private readonly Func<CultureInfo> currentCultureGetter;

        public Localizer(string resourceSubTree, Func<ResourceLoader> resourceManagerGetter, Func<CultureInfo> currentCultureGetter)
        {
            this.resourceSubTree = resourceSubTree;
            this.resourceManagerGetter = resourceManagerGetter;
            this.currentCultureGetter = currentCultureGetter;
        }

        public ResourceLoader ResourceLoader
        {
            get
            {
                return resourceManagerGetter();
            }
        }

        public CultureInfo CurrentCulture
        {
            get
            {
                return currentCultureGetter();
            }
        }

        public string GetLocalizedValue(object value, params object[] args)
        {
            return GetLocalizedValue(value, null, null, args);
        }

        public string GetLocalizedValue(object value, string suffix, params object[] args)
        {
            return GetLocalizedValue(value, suffix, null, args);
        }

        public string GetLocalizedValue(object value, string suffix, string culture, params object[] args)
        {
            if (value == null)
            {
                return "<!! NULL !!>";
            }
            ILocalizable localizableValue = value as ILocalizable;
            string localizedValue;
            object valueKey = Tuple.Create(value, suffix);
            if (!localizedValueMap.TryGetValue(valueKey, out localizedValue))
            {
                string valueType = value.GetType().Name;
                string valueName = value.ToString();
                Enum enumValue = value as Enum;
                bool isFlags = enumValue != null && enumValue.GetType().GetTypeInfo().GetCustomAttributes<FlagsAttribute>().Any();
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
                else if (localizableValue != null)
                {
                    // If it's localized, then we'll let them determine
                    // the appropriate way to get the raw localized value
                    localizedValue = localizableValue.ToString(this);
                }
                else
                {
                    string localizedValueKey = string.Format("{0}_{1}", valueType, valueName);
                    if (!string.IsNullOrWhiteSpace(suffix))
                    {
                        localizedValueKey = localizedValueKey + "_" + suffix;
                    }

                    ResourceContext context = new ResourceContext();
                    context.Languages = new string[] { ResolveCulture(culture).ToString() };
                    ResourceMap map = ResourceManager.Current.MainResourceMap.GetSubtree(this.resourceSubTree);
                    ResourceCandidate localizedCandidate = map.GetValue(localizedValueKey, context);
                    
                    localizedValue = localizedCandidate.ValueAsString ?? "!! Missing Resource for " + localizedValueKey + " !!";
                    //localizedValue = ResourceManager.GetString(localizedValueKey, ResolveCulture(culture)) ?? "!! Missing Resource for " + localizedValueKey + " !!";
                }
                localizedValueMap[valueKey] = localizedValue;
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

        private string GetRawLocalizedValue(object value, string culture)
        {
            string localizedValue;
            if (!localizedValueMap.TryGetValue(value, out localizedValue))
            {
                string valueType = value.GetType().Name;
                string valueName = value.ToString();
                string localizedValueKey = string.Format("{0}_{1}", valueType, valueName);
                localizedValue = this.ResourceLoader.GetString(localizedValueKey);
                //localizedValue = ResourceManager.GetString(localizedValueKey, ResolveCulture(culture)) ?? "!! Missing Resource !!";
                localizedValueMap[localizedValueKey] = localizedValue;
            }
            return localizedValue;
        }

        private CultureInfo ResolveCulture(string culture)
        {
            CultureInfo cultureInfo = string.IsNullOrWhiteSpace(culture) ? null : new CultureInfo(culture);
            // If a culture override is provided on the ResourceManager, then we just want to use that one
            // Otherwise use the culture provided and default to the invariant culture otherwise.
            return this.CurrentCulture ?? cultureInfo ?? CultureInfo.InvariantCulture;
        }

    }

    public static class Localized
    {
        public static Localizer Strings = new Localizer("DominionCore/Strings", () => ResourceLoader.GetForViewIndependentUse("Strings"), () => Dominion.Resources.Strings.Culture);
        public static Localizer CardData = new Localizer("DominionCore/CardDataStrings", () => ResourceLoader.GetForViewIndependentUse("CardDataStrings"), () => CardDataStrings.Culture);
    }

    public interface ILocalizable
    {

        object[] LocalizedContext { get; }

        string ToString(Localizer localizer);

    }

}