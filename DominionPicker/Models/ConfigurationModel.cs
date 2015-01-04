using System.Globalization;
using System.IO.IsolatedStorage;
using Ben.Dominion.Resources;
using Ben.Utilities;

namespace Ben.Dominion.Models
{
    /// <summary>
    /// Stores all of the application level configuration
    /// </summary>
    public class ConfigurationModel : NotifyPropertyChangedBase
    {
        public const string ConfigurationModelFilePath = "Configuration.xml";
        private static ConfigurationModel instance;
        private CultureInfo overrideCulture = CultureInfo.InvariantCulture;

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

        public CultureInfo CurrentCulture
        {
            get { return this.OverrideCulture ?? CultureInfo.CurrentCulture; }

            set { this.OverrideCulture = value; }
        }

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
                if (value == CultureInfo.CurrentCulture)
                {
                    value = null;
                }

                this.OverrideCultureName = value != null ? value.Name : null;
                this.NotifyPropertyChanged("OverrideCulture");
                this.NotifyPropertyChanged("CurrentCulture");
            }
        }

        public string OverrideCultureName
        {
            get
            {
                return
                    IsolatedStorageSettings.ApplicationSettings.TryGetOrDefault<string>(
                        "Application_OverrideCultureName");
            }

            set
            {
                IsolatedStorageSettings.ApplicationSettings["Application_OverrideCultureName"] = value;
                this.overrideCulture = CultureInfo.InvariantCulture;
                this.NotifyPropertyChanged("OverrideCultureName");
            }
        }

        public bool LocalizeUI
        {
            get { return IsolatedStorageSettings.ApplicationSettings.TryGetOrDefault("Application_LocalizeUI", true); }

            set
            {
                IsolatedStorageSettings.ApplicationSettings["Application_LocalizeUI"] = value;
                this.NotifyPropertyChanged("LocalizeUI");
            }
        }

        public bool LocalizeCardData
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings.TryGetOrDefault("Application_LocalizeCardData", true);
            }

            set
            {
                IsolatedStorageSettings.ApplicationSettings["Application_LocalizeCardData"] = value;
                this.NotifyPropertyChanged("LocalizeCardData");
            }
        }

        public bool LocalizeRulesText
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings.TryGetOrDefault("Application_LocalizeRulesText", true);
            }

            set
            {
                IsolatedStorageSettings.ApplicationSettings["Application_LocalizeRulesText"] = value;
                this.NotifyPropertyChanged("LocalizeRulesText");
            }
        }

        private static ConfigurationModel Load()
        {
            return Load(ConfigurationModelFilePath);
        }

        private static ConfigurationModel Load(string path)
        {
            return new ConfigurationModel();
        }
    }
}
