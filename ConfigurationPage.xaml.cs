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

            this.CulturesListPicker.ItemsSource = new List<CultureInfo>
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
                // THIS IS NOT A REAL CULTURE JUST THE LOCALIZATION TEST
                new CultureInfo("ru-RU"),
            }.OrderBy(c => c.DisplayName);
        }
    }
}