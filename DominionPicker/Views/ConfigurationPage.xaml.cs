using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ben.Dominion.ViewModels;
using Microsoft.Phone.Controls;

namespace Ben.Dominion
{
    public partial class ConfigurationPage : PhoneApplicationPage
    {
        public ConfigurationPage()
        {
            this.InitializeComponent();

            this.DataContext = ConfigurationModel.Instance;

	        this.OwnedSets.SummaryForSelectedItemsDelegate = list =>
	        {
		        if (list == null || list.Count == 0)
		        {
			        return "None";
		        }

		        if (list.Count == Cards.AllSets.Count)
		        {
			        return "Everything";
		        }

                if(list.Count == 1)
                {
                    return "1 set";
                }

                return string.Format("{0} sets", list.Count);

		        //var setNames = list.Cast<CardSet>().Select(s => s.Localize().Substring(0,3));
		        //return string.Join(", ", setNames);
	        };

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