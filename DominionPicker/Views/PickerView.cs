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
        Main,
        /// <summary>
        /// Navigate to the selected cards results list 
        /// </summary>
        [Description("ResultsViewer.xaml")]
        Results,
        /// <summary>
        /// Navigate to the card info details page
        /// </summary>
        [Description("CardInfo.xaml")]
        CardInfo,
        /// <summary>
        /// Navigate to the card lookup page.
        /// </summary>
        [Description("CardFilterPage.xaml")]
        CardFilter,
        /// <summary>
        /// Navigate to the black market page.
        /// </summary>
        [Description("BlackMarketPage.xaml")]
        BlackMarket,
        [Description("ImportExportPage.xaml")]
        ImportExport,
        [Description("ConfigurationPage.xaml")]
        Settings,
        [Description("AboutPage.xaml")]
        About,
        [Description("DebugPage.xaml")]
        Debug,
    }

    public static class PickerViews
    {
        public static void RegisterAll()
        {
            NavigationServiceHelper.RegisterAll<PickerView>();

            Type type = typeof (PickerView);
            var views = Enum.GetValues(type).Cast<PickerView>();

            foreach (PickerView view in views)
            {
                MemberInfo viewInfo = type.GetMember(view.ToString()).FirstOrDefault();
                DescriptionAttribute description =
                    (DescriptionAttribute) viewInfo.GetCustomAttributes(typeof (DescriptionAttribute)).FirstOrDefault();

                if (description != null)
                {
                    
                    //throw new NotImplementedException();
                    ////NavigationServiceHelper<PickerView>.RegisterView(view, description.Description.GetType());
                }
            }
        }

        public static void Go(this PickerView view)
        {
            NavigationServiceHelper<PickerView>.Navigate(view);
        }
    }
}