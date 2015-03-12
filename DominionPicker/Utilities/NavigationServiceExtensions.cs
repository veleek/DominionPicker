using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GoogleAnalytics;

namespace Ben.Utilities
{
    public class NavigationServiceHelper
    {
        private static bool isInitialized;
        private static Frame frame;

        protected NavigationServiceHelper()
        { }
        
        public static bool IsNavigating { get; private set; }

        public static void Initialize()
        {
            if (!isInitialized)
            {
                var rootFrame = Application.Current.RootVisual as Frame;

                if (rootFrame == null)
                {
                    throw new InvalidOperationException(
                        "Application.Current.RootVisual must be of type Frame to use NavigationServiceEx");
                }

                Initialize(frame);
            }
        }

        public static void Initialize(Frame rootFrame)
        {
            if (!isInitialized)
            {
                frame = rootFrame;
                frame.Navigating += Frame_Navigating;
                frame.Navigated += Frame_Navigated;
                frame.NavigationStopped += Frame_NavigationStopped;
                frame.NavigationFailed += Frame_NavigationFailed;
                
                isInitialized = true;
            }
        }

        private static void Frame_Navigating(Object sender, NavigatingCancelEventArgs e)
        {
            AppLog.Instance.Debug("Navigating: {0}, Mode: {1}, AppInitiated? {2}", e.Uri, e.NavigationMode, e.IsNavigationInitiator);
        }

        private static void Frame_Navigated(Object sender, NavigationEventArgs e)
        {
            AppLog.Instance.Debug("Navigated: {0}, Mode: {1}, AppInitiated? {2}", e.Uri, e.NavigationMode, e.IsNavigationInitiator);

            IsNavigating = false;

            if (!e.Uri.IsAbsoluteUri || e.Uri.AbsoluteUri != "app://external/")
            {
                EasyTracker.GetTracker().SendView(e.Uri.ToString());
            }
        }

        private static void Frame_NavigationStopped(Object sender, NavigationEventArgs e)
        {
            AppLog.Instance.Debug("NavigationStopped: {0}, Mode: {1}, AppInitiated? {2}", e.Uri, e.NavigationMode, e.IsNavigationInitiator);

            IsNavigating = false;
        }

        private static void Frame_NavigationFailed(Object sender, NavigationFailedEventArgs e)
        {
            AppLog.Instance.Debug("NavigationFailed: {0}, Error: {1}", e.Uri, e.Exception.Message);

            IsNavigating = false;
        }

        public static void Navigate(String pageUri)
        {
            Navigate(new Uri("/Views" + pageUri, UriKind.Relative));
        }


        public static void Navigate(Uri pageUri)
        {
            Initialize();

            if (IsNavigating)
            {
                // Just ignore multiple navigation requests for now
                return;
            }

            IsNavigating = true;

            try
            {
                frame.Navigate(pageUri);
            }
            catch (InvalidOperationException ioe)
            {
                IsNavigating = false;

                // Occasionally we can get an exception here where navigation will fail
                // if the app is no longer in the foreground for some reason.  Just ignore it.
                AppLog.Instance.Error("Caught navigation exception", ioe);
            }
        }

        /// <summary>
        /// Simple helper method that allows us to navigate to a page using a strongly typed
        /// page reference without needing to explicitly specify the type twice.
        /// </summary>
        /// <typeparam name="TView">The type of the view to navigate to</typeparam>
        /// <param name="view">The view to navigate to</param>
        public static void Navigate<TView>(TView view)
        {
            NavigationServiceHelper<TView>.Navigate(view);
        }

        public static void CancelNavigation()
        {
            frame.StopLoading();
        }
    }

    public class NavigationServiceHelper<TView> : NavigationServiceHelper
    {
        private static readonly Dictionary<TView, Uri> RegisteredViews = new Dictionary<TView, Uri>();

        protected NavigationServiceHelper() { }

        public static void RegisterView(TView view, string pageUri)
        {
            if (RegisteredViews.ContainsKey(view))
            {
                throw new ArgumentException(string.Format("View {0} is already registered to {1}", view, RegisteredViews[view]), "view");
            }

            if (pageUri == null)
            {
                throw new ArgumentNullException("pageUri");
            }

            RegisteredViews[view] = new Uri("/Views/" + pageUri, UriKind.Relative);
        }

        public static void Navigate(TView view)
        {
            Uri uri;
            if (!RegisteredViews.TryGetValue(view, out uri))
            {
                throw new ArgumentException(string.Format("View {0} is not registered", view));
            }

            Navigate(uri);
        }

    }

    public static class NavigationServiceExtensions
    {
        public static void Navigate(this NavigationService navigationService, String pageUri)
        {
            NavigationServiceHelper.Navigate(pageUri);
        }
    }
}