using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;
#if NETFX_CORE
using Windows.ApplicationModel.Resources.Core;
using ResourceManager = Windows.ApplicationModel.Resources.ResourceLoader;
#else
using System.Resources;
#endif

namespace Ben.Data
{

    public class Localizer
    {
        private static Dictionary<object, string> localizedValueMap = new Dictionary<object, string>();
        private readonly string resourceSubTree;
        private readonly Func<ResourceManager> resourceManagerGetter;
        private readonly Func<CultureInfo> currentCultureGetter;
#if NETFX_CORE
        private readonly ResourceMap map;
#endif

        public Localizer(string resourceSubTree, Func<ResourceManager> resourceManagerGetter, Func<CultureInfo> currentCultureGetter)
        {
            this.resourceSubTree = resourceSubTree;
            this.resourceManagerGetter = resourceManagerGetter;
            this.currentCultureGetter = currentCultureGetter;
#if NETFX_CORE
            this.map = Windows.ApplicationModel.Resources.Core.ResourceManager.Current.MainResourceMap.GetSubtree(this.resourceSubTree);
#endif
        }

        public ResourceManager ResourceLoader
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

#if NETFX_CORE
                    ResourceContext context = new ResourceContext();
                    context.Languages = new string[] { ResolveCulture(culture).ToString() };
                    ResourceCandidate localizedCandidate = map.GetValue(localizedValueKey, context);
                    
                    if(localizedCandidate == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Unable to find localized value {localizedValueKey}");
                    }

                    localizedValue = localizedCandidate?.ValueAsString ?? "!! Missing Resource for " + localizedValueKey + " !!";
#else
                    localizedValue = this.ResourceLoader.GetString(localizedValueKey, ResolveCulture(culture)) ?? "!! Missing Resource for " + localizedValueKey + " !!";
#endif
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
}