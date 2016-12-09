using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Serialization;
using Ben.Dominion.Resources;
using Ben.Utilities;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Ben.Dominion.ViewModels
{
    /// <summary>
    /// Stores all of the application level configuration
    /// </summary>
    public class ConfigurationModel
       : NotifyPropertyChangedBase
    {
        public const string ConfigurationModelFilePath = "Configuration.xml";
        private static ConfigurationModel instance;
        private CultureInfo overrideCulture = CultureInfo.InvariantCulture;
        private List<CardSet> ownedSets = Cards.AllSets.ToList();

        public static ConfigurationModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Load();
                    if (instance.LocalizeUI)
                    {
                        Strings.Culture = instance.CurrentCulture;
                    }
                    if (instance.LocalizeCardData)
                    {
                        CardDataStrings.Culture = instance.CurrentCulture;
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Convenience accessor to allow binding to the set of all cards.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<CardSet> AllSets
        {
            get
            {
                return Cards.AllSets;
            }
        }

        [XmlIgnore]
        public List<CardSet> OwnedSets
        {
            get
            {
                return this.ownedSets;
            }
            set
            {
                this.SetProperty(ref this.ownedSets, value);
            }
        }

        [XmlElement("OwnedSets")]
        public string OwnedSetNames
        {
            get
            {
                return string.Join(",", this.OwnedSets.Select(s => s.ToString()));
            }
            set
            {
                this.OwnedSets = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => (CardSet)Enum.Parse(typeof(CardSet), s, true)).ToList();
            }
        }

        [XmlIgnore]
        public List<CultureInfo> SupportedCultures { get; } = new List<CultureInfo>
        {
            //null,
            new CultureInfo("cs-CZ"),
            new CultureInfo("de-DE"),
            new CultureInfo("en-US"),
            new CultureInfo("es-ES"),
            new CultureInfo("fi-FI"),
            new CultureInfo("fr-FR"),
            new CultureInfo("it-IT"),
            new CultureInfo("nl-NL"),
            new CultureInfo("pl-PL"),
#if DEBUG
            // Add the psudolocalization culture.
            new CultureInfo("qps-ploc")
#endif
        }.OrderBy(c => c.TwoLetterISOLanguageName, StringComparer.OrdinalIgnoreCase).ToList();

        [XmlIgnore]
        public CultureInfo CurrentCulture
        {
            get
            {
                return this.OverrideCulture ?? CultureInfo.CurrentCulture;
            }
            set
            {
                this.OverrideCulture = value;
            }
        }

        [XmlIgnore]
        /// <summary>
        /// The culture info used to display localized details instead of the currently configured culture
        /// </summary>
        public CultureInfo OverrideCulture
        {
            get
            {
                if (Equals(this.overrideCulture, CultureInfo.InvariantCulture))
                {
                    var cultureName = this.OverrideCultureName;
                    this.overrideCulture = cultureName != null ? new CultureInfo(cultureName) : null;
                }
                return this.overrideCulture;
            }
            set
            {
                if (Equals(value, CultureInfo.CurrentCulture))
                {
                    value = null;
                }
                this.OverrideCultureName = value != null ? value.Name : null;
                this.OnPropertyChanged();
                this.OnPropertyChanged("CurrentCulture");
            }
        }

        public string OverrideCultureName
        {
            get
            {
                return this.GetAppSetting<string>();
            }
            set
            {
                if (this.SetAppSetting(value))
                {
                    this.overrideCulture = CultureInfo.InvariantCulture;
                }
            }
        }

        public bool LocalizeUI
        {
            get
            {
                return this.GetAppSetting(defaultValue: true);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        public bool LocalizeCardData
        {
            get
            {
                return this.GetAppSetting(defaultValue: true);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        public bool LocalizeRulesText
        {
            get
            {
                return this.GetAppSetting(defaultValue: true);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how to pick whether to include Platinums and Colonies.
        /// </summary>
        public PlatinumColonyOption PickPlatinumColony
        {
            get
            {
                return this.GetAppSetting(defaultValue: PlatinumColonyOption.Randomly);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how to pick whether to replace Estates with Shelters in the starting hands.
        /// </summary>
        public SheltersOption PickSheltersOrEstates
        {
            get
            {
                return this.GetAppSetting(defaultValue: SheltersOption.Randomly);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        public EventsOption PickEvents
        {
            get
            {
                return this.GetAppSetting(defaultValue: EventsOption.Randomly);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not extra bits and pieces should be shown in the results list.
        /// </summary>
        public bool ShowExtras
        {
            get
            {
                return this.GetAppSetting(defaultValue: true);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not all the basic treasures and victory cards are shown and
        /// the starting hand is created.
        /// </summary>
        public bool ShowBasicCards
        {
            get
            {
                return GetAppSetting(defaultValue: true);
            }
            set
            {
                this.SetAppSetting(value);
            }
        }

        private bool GetAppSetting([CallerMemberName]string key = null, bool defaultValue = true)
        {
            return GetAppSetting<bool>(key, defaultValue);
            //string settingValue = GetAppSetting(key, defaultValue.ToString());
            //return Boolean.Parse(settingValue);
        }


#if NETFX_CORE
        private TValue GetAppSetting<TValue>([CallerMemberName]string key = null, TValue defaultValue = default(TValue))
        {
            return ApplicationData.Current.LocalSettings.TryGetOrDefault("Application_" + key, defaultValue);
        }

        private bool SetAppSetting<TValue>(TValue value, [CallerMemberName]string key = null)
        {
            bool updated = ApplicationData.Current.LocalSettings.TryReplace("Application_" + key, value);
            if (updated)
            {
                this.OnPropertyChanged(key);
            }

            return updated;
        }
#else
        private TValue GetAppSetting<TValue>([CallerMemberName]string key = null, TValue defaultValue = default(TValue))
        {
            
            return IsolatedStorageSettings.ApplicationSettings.TryGetOrDefault("Application_" + key, defaultValue);
        }

        private bool SetAppSetting<TValue>(TValue value, [CallerMemberName]string key = null)
        {
            TValue current = (TValue)IsolatedStorageSettings.ApplicationSettings["Application_" + key];

            if (Equals(current, value)) {
                return false;
            }

            IsolatedStorageSettings.ApplicationSettings["Application_" + key] = value;
            this.OnPropertyChanged(key);

            return true;
        }
#endif

        

        private static ConfigurationModel Load()
        {
            return Load(ConfigurationModelFilePath);
        }

        private static ConfigurationModel Load(string path)
        {
            try
            {
                return new ConfigurationModel();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }
    }
}