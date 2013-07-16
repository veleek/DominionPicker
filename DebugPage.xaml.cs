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
using Ben.Utilities;
using System.Globalization;
using System.Threading;
using Ben.Dominion.Resources;

namespace Ben.Dominion
{
    public partial class DebugPage : PhoneApplicationPage
    {
        public DebugPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DebugPage_Loaded);
            this.DataContext = AppLog.Instance;
        }

        void DebugPage_Loaded(object sender, RoutedEventArgs e)
        {
            AppLog.Instance.Log("Strings.Title: {0} Culture: {1}", Strings.Application_Title, Thread.CurrentThread.CurrentCulture.Name);

            this.CulturePicker.ItemsSource = new List<string>
            {
                "en-US",
                "en-GB",
                "fi-FI",
                "fr-FR",
                "zh-CN",
                "zh-TW",
                "cs-CZ",
                "da-DK",
                "nl-NL",
                "de-DE",
                "el-GR",
                "hu-HU",
                "id-ID",
                "it-IT",
                "ja-JP",
                "ko-KR",
                "ms-MY",
                "nb-NO",
                "pl-PL",
                "pt-BR",
                "pt-PT",
                "ru-RU",
                "es-ES",
                "sv-SE",
            };

            this.CulturePicker.SelectedItem = CultureInfo.CurrentCulture.Name;
        }

        private void CulturePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cul = CulturePicker.SelectedItem as string;

            if (cul == null)
            {
                return;
            }

            // set this thread's current culture to the culture associated with the selected locale
            CultureInfo newCulture = new CultureInfo(cul);
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;

            CultureInfo cc, cuic;
            cc = Thread.CurrentThread.CurrentCulture;
            cuic = Thread.CurrentThread.CurrentUICulture;

            /*
            // display the culture name in the language of the selected locale
            regionalFrmt.Text = cc.NativeName;

            // display the culture name in the localized language
            displayLang.Text = cuic.DisplayName;

            // display the date formats (long and short form) for the  current culuture  
            DateTime curDate = DateTime.Now;
            longDate.Text = cc.DateTimeFormat.LongDatePattern.ToString() + " " + curDate.ToString("D");
            shortDate.Text = cc.DateTimeFormat.ShortDatePattern.ToString() + "   " + curDate.ToString("d");

            // display the time format (long form) for the current culture
            longTime.Text = cc.DateTimeFormat.LongTimePattern + "   " + curDate.ToString("T");

            // display the currency format and currency symbol for the current culture
            Int64 money = 123456789;
            currencyFrmt.Text = money.ToString("C");
             */

            AppLog.Instance.Log("Strings.Title: {0} Culture: {1}", Strings.Application_Title, Thread.CurrentThread.CurrentCulture.Name);
        }
    }
}