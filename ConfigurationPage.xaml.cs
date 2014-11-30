using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ben.Dominion.Models;
using Microsoft.Phone.Controls;

namespace Ben.Dominion
{
    public partial class ConfigurationPage : PhoneApplicationPage
    {
        public ConfigurationPage()
        {
            this.InitializeComponent();

            this.DataContext = ConfigurationModel.Instance;

            var supportedCultures = new List<CultureInfo>
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
            };

            #if DEBUG
            // THIS IS NOT A REAL CULTURE JUST THE LOCALIZATION TEST
            supportedCultures.Add(new CultureInfo("ru-RU"));
            #endif

            this.CulturesListPicker.ItemsSource = supportedCultures.OrderBy(c => c.TwoLetterISOLanguageName, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}