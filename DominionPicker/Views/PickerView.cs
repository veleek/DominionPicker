using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Ben.Utilities;

namespace Ben.Dominion.Views
{
    public enum PickerView
    {
        None,
        /// <summary>
        /// Navigate to the Main view page
        /// </summary>
        [Description("MainPage.xaml")]
        [ViewType(typeof(MainPage))]
        Main,
        /// <summary>
        /// Navigate to the selected cards results list 
        /// </summary>
        [Description("ResultsViewer.xaml")]
        [ViewType(typeof(ResultsViewer))]
        Results,
        /// <summary>
        /// Navigate to the card info details page
        /// </summary>
        [Description("CardInfo.xaml")]
        [ViewType(typeof(CardInfo))]
        CardInfo,
        /// <summary>
        /// Navigate to the card lookup page.
        /// </summary>
        [Description("CardFilterPage.xaml")]
        [ViewType(typeof(CardFilterPage))]
        CardFilter,
        /// <summary>
        /// Navigate to the black market page.
        /// </summary>
        [Description("BlackMarketPage.xaml")]
        [ViewType(typeof(BlackMarketPage))]
        BlackMarket,
        [Description("ImportExportPage.xaml")]
        [ViewType(typeof(ImportExportPage))]
        ImportExport,
        [Description("ConfigurationPage.xaml")]
        [ViewType(typeof(ConfigurationPage))]
        Settings,
        [Description("AboutPage.xaml")]
        [ViewType(typeof(AboutPage))]
        About,
        [Description("DebugPage.xaml")]
        Debug,
    }

    public static class PickerViewExtensions
    {
        public static void Go(this PickerView view)
        {
            NavigationServiceHelper<PickerView>.Navigate(view);
        }
    }
}