using Ben.Utilities;

namespace Ben.Dominion.Views
{
    public enum PickerView
    {
        None,
        /// <summary>
        /// Navigate to the Main view page
        /// </summary>
        [ViewType(typeof(MainPage))]
        Main,
        /// <summary>
        /// Navigate to the selected cards results list 
        /// </summary>
        [ViewType(typeof(ResultsViewer))]
        Results,
        /// <summary>
        /// Navigate to the card info details page
        /// </summary>
        [ViewType(typeof(CardInfo))]
        CardInfo,
        /// <summary>
        /// Navigate to the card lookup page.
        /// </summary>
        [ViewType(typeof(CardLookupPage))]
        CardLookup,
        /// <summary>
        /// Navigate to the black market page.
        /// </summary>
        [ViewType(typeof(BlackMarketPage))]
        BlackMarket,
        /// <summary>
        /// Navigate to the settings page.
        /// </summary>
        [ViewType(typeof(ConfigurationPage))]
        Settings,
        /// <summary>
        /// Navigate to the About Page.
        /// </summary>
        [ViewType(typeof(AboutPage))]
        About,
        Debug,
    }

    public static class PickerViews
    {
        static PickerViews()
        {
            NavigationServiceHelper.RegisterAll<PickerView>();
        }

        /// <summary>
        /// This method does nothing, it just provides a method we can call to ensure 
        /// that the static initializer gets called.
        /// </summary>
        public static void Initialize()
        {
        }

        /// <summary>
        /// Extension method for PickerView to make it easy to navigate to the appropriate
        /// view associated with the given enum.
        /// </summary>
        /// <param name="view"></param>
        public static void Go(this PickerView view, object parameter = null)
        {
            NavigationServiceHelper.Navigate(view, parameter);
        }
    }
}