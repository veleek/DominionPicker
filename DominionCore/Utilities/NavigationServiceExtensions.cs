using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
#endif 

namespace Ben.Utilities
{

    public class NavigationServiceHelper
    {
        private static bool isInitialized;
        private static Frame frame;

        public static bool IsNavigating { get; private set; }

        public static void Initialize()
        {
            if (!isInitialized)
            {
#if NETFX_CORE
                var rootFrame = Window.Current.Content as Frame;
#else
                var rootFrame = Application.Current.RootVisual as Frame;
#endif
                if (rootFrame == null)
                {
                    throw new InvalidOperationException("Application.Current.RootVisual must be of type Frame to use NavigationServiceEx");
                }
                Initialize(rootFrame);
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
#if NETFX_CORE
            Uri uri = new Uri("/" + e.SourcePageType.Name + ".xaml", UriKind.Relative);
#else
            Uri uri = e.Uri;
#endif
            AppLog.Instance.Debug("Navigating: {0}, Mode: {1}, AppInitiated? {2}", uri, e.NavigationMode, "");
        }

        private static void Frame_Navigated(Object sender, NavigationEventArgs e)
        {
#if NETFX_CORE
            Uri uri = new Uri("/" + e.SourcePageType.Name + ".xaml", UriKind.Relative);
#else
            Uri uri = e.Uri;
#endif

            AppLog.Instance.Debug("Navigated: {0}, Mode: {1}, AppInitiated? {2}", uri, e.NavigationMode, "");
            IsNavigating = false;
            if (!uri.IsAbsoluteUri || uri.AbsoluteUri != "app://external/")
            {
                //EasyTracker.GetTracker().SendView(new Uri("/" + e.SourcePageType.Name + ".xaml", UriKind.Relative).ToString());
            }
        }

        private static void Frame_NavigationStopped(Object sender, NavigationEventArgs e)
        {
#if NETFX_CORE
            Uri uri = new Uri("/" + e.SourcePageType.Name + ".xaml", UriKind.Relative);
#else
            Uri uri = e.Uri;
#endif
            AppLog.Instance.Debug("NavigationStopped: {0}, Mode: {1}, AppInitiated? {2}", uri, e.NavigationMode, "");
            IsNavigating = false;
        }

        private static void Frame_NavigationFailed(Object sender, NavigationFailedEventArgs e)
        {
            AppLog.Instance.Debug("NavigationFailed. Error: {0}", e.Exception.Message);
            IsNavigating = false;
        }

        public static void Navigate(String pageType, object parameter = null)
        {
            Navigate(Type.GetType(pageType), parameter);
        }

        public static void Navigate(Type pageType, object parameter = null)
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
                throw new NotImplementedException();
                //frame.Navigate(pageType, parameter);
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
        public static void Navigate<TView>(TView view, object parameter = null)
        {
            NavigationServiceHelper<TView>.Navigate(view, parameter);
        }

        public static void CancelNavigation()
        {
            //frame.StopLoading();
        }


        public static void RegisterAll<TView>()
        {
            var views = Enum.GetValues(typeof(TView)).Cast<TView>();
            foreach (TView view in views)
            {
                MemberInfo viewInfo = typeof(TView).GetMember(view.ToString()).FirstOrDefault();
                ViewTypeAttribute viewType = viewInfo.GetCustomAttributes<ViewTypeAttribute>().FirstOrDefault();
                if (viewType != null)
                {
                    NavigationServiceHelper<TView>.RegisterView(view, viewType.ViewType);
                }
            }
        }
    }

    public class NavigationServiceHelper<TView> : NavigationServiceHelper
    {
        private static readonly Dictionary<TView, Type> RegisteredViews = new Dictionary<TView, Type>();

        protected NavigationServiceHelper()
        {
        }

        public static void RegisterView(TView view, Type viewType)
        {
            if (RegisteredViews.ContainsKey(view))
            {
                throw new ArgumentException(string.Format("View {0} is already registered to {1}", view, RegisteredViews[view]), "view");
            }

            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }

            RegisteredViews[view] = viewType;
        }

        public static void Navigate(TView view, object parameter = null)
        {
            Type viewType;
            if (!RegisteredViews.TryGetValue(view, out viewType))
            {
                throw new ArgumentException($"View {view} is not registered");
            }

            Navigate(viewType, parameter);
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ViewTypeAttribute : Attribute
    {
        public ViewTypeAttribute(Type viewType)
        {
            this.ViewType = viewType;
        }

        public Type ViewType { get; private set; }
    }
}