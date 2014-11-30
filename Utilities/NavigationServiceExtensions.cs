using System;
using System.Windows.Navigation;

namespace Ben.Utilities
{
    public static class NavigationServiceExtensions
    {
        private static bool isInitalized;

        public static void Initialize(this NavigationService navigationService)
        {
            navigationService.Navigated += navigationService_Navigated;
            isInitalized = true;
        }

        static void navigationService_Navigated(object sender, NavigationEventArgs e)
        {
            IsNavigating = false;
        }

        public static bool IsNavigating { get; private set; }

        public static void Navigate(this NavigationService navigationService, String pageUri)
        {
            if (!isInitalized)
            {
                Initialize(navigationService);
            }

            if(IsNavigating)
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
            catch(InvalidOperationException ioe)
            {
                IsNavigating = false;

                // Occasionally we can get an exception here where navigation will fail
                // if the app is no longer in the foreground for some reason.  Just ignore it.
                BugSense.BugSenseHandler.Instance.LogEvent(string.Format("Caught navigation exception: {0}", ioe.Message));
            }

        }
    }
}