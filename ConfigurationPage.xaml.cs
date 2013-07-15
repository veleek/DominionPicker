using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Globalization;

namespace Ben.Dominion
{
    public partial class ConfigurationPage : PhoneApplicationPage
    {
        public ConfigurationPage()
        {
            InitializeComponent();

            this.DataContext = MainModel.Instance.Configuration;

            CulturesListPicker.ItemsSource = new List<CultureInfo>
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