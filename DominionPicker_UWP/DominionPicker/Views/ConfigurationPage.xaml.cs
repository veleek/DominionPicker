using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ben.Dominion.ViewModels;

namespace Ben.Dominion
{

    public partial class ConfigurationPage
       : Windows.UI.Xaml.Controls.Page
    {

        public ConfigurationPage()
        {
            this.InitializeComponent();
            this.DataContext = ConfigurationModel.Instance;
            //this.OwnedSets.SummaryForSelectedItemsDelegate = list =>
            //   {
            //      if ( list == null || list.Count == 0 )
            //      {
            //         return "None";
            //      }
            //      if ( list.Count == Cards.AllSets.Count )
            //      {
            //         return "Everything";
            //      }
            //      if ( list.Count == 1 )
            //      {
            //         return "1 set";
            //      }
            //      return string.Format("{0} sets", list.Count);
            //   //var setNames = list.Cast<CardSet>().Select(s => s.Localize().Substring(0,3));
            //   //return string.Join(", ", setNames);
            //   };
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
            // Add the psudolocalization culture.
            supportedCultures.Add(new CultureInfo("qps-ploc"));
            #endif

            this.CulturesListPicker.ItemsSource = supportedCultures.OrderBy(c => c.TwoLetterISOLanguageName, StringComparer.OrdinalIgnoreCase);
        }
    }

}