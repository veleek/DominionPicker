using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Ben.Dominion.Resources;
using Ben.Utilities;

namespace Ben.Dominion
{
    public partial class DebugPage
    {
        public DebugPage()
        {
            this.InitializeComponent();
            this.Loaded += this.DebugPage_Loaded;
            this.DataContext = AppLog.Instance;
        }

        private void DebugPage_Loaded(object sender, RoutedEventArgs e)
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
            string cul = this.CulturePicker.SelectedItem as string;

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

        private void SavePickerState_OnClick(object sender, RoutedEventArgs e)
        {
            string tag = sender.GetTag<string>();
            string[] tagParts = tag.Split(',');

            using (var savedStateStream = Microsoft.Xna.Framework.TitleContainer.OpenStream(tagParts[0]))
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (Stream currentStateStream = store.OpenFile(tagParts.Length > 1 ? tagParts[1] : "PickerState.xml", FileMode.Create))
                    {
                        savedStateStream.CopyTo(currentStateStream);
                    }
                }
            }
        }
    }
}