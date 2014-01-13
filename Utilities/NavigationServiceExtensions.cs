using System;
using System.Windows.Navigation;

namespace Ben.Utilities
{
    public static class NavigationServiceExtensions
    {
        private static bool isInitalized = false;

        public static void Initialize(this NavigationService navigationService)
        {
            navigationService.Navigated += new NavigatedEventHandler(navigationService_Navigated);
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
            Uri navUri = new Uri(pageUri, UriKind.Relative);
            navigationService.Navigate(navUri);
        }
    }
}