using System;
using System.Windows.Navigation;
using GoogleAnalytics;

namespace Ben.Utilities
{
    public static class NavigationServiceHelper
    {
        private static NavigationService cachedNavigationService;
        private static bool isInitalized;

        public static void Initialize(this NavigationService navigationService)
        {
            if (cachedNavigationService != null && navigationService != cachedNavigationService)
            {
                throw new ArgumentException("Uh oh! We have multiple instances of NavigationService");    
            }

            if (!isInitalized)
            {
                cachedNavigationService = navigationService;
                cachedNavigationService.Navigated += navigationService_Navigated;
                isInitalized = true;
            }
        }

        private static void navigationService_Navigated(object sender, NavigationEventArgs e)
        {
            IsNavigating = false;

            if (!e.Uri.IsAbsoluteUri || e.Uri.AbsoluteUri != "app://external/")
            {
                EasyTracker.GetTracker().SendView(e.Uri.ToString());
            }
        }

        public static bool IsNavigating { get; private set; }

        public static void Navigate(this NavigationService navigationService, String pageUri)
        {
            Initialize(navigationService);

            if (IsNavigating)
            {
                // Just ignore multiple navigation requests for now
                return;
            }

            IsNavigating = true;

            try
            {
                Uri navUri = new Uri(pageUri, UriKind.Relative);
                navigationService.Navigate(navUri);
            }
            catch (InvalidOperationException ioe)
            {
                IsNavigating = false;

                // Occasionally we can get an exception here where navigation will fail
                // if the app is no longer in the foreground for some reason.  Just ignore it.
                AppLog.Instance.Error("Caught navigation exception", ioe);
            }
        }

        public static void Navigate(String pageUri)
        {
            cachedNavigationService.Navigate(pageUri);   
        }
    }
}