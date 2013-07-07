using System;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using com.mtiks.winmobile;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Shell;
using System.IO;
using Ben.Utilities;

namespace Ben.Dominion
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }
        
        public Card SelectedCard { get; set; }

        //public static readonly String AdApplicationId = "a7ac8297-9d02-405b-9dca-4d702cf50997";
        public static readonly String MtiksApplicationId = "166ff6d5917b9569d549eec40";

        private LicenseInformation license = new LicenseInformation();
        public Boolean IsTrial
        {
            get
            {
                //return true;
                return license.IsTrial();
            }
        }

        private Boolean isNew = false;

        public bool IsNewVersion { get; set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            isNew = true;

            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                //MetroGridHelper.IsVisible = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            var instance = MainViewModel.Instance;

            var s = new PickerState
            {
                CurrentSettings = new PickerSettings(),
                FavoriteSets =
                {
                    new OldFavoriteSet("Test", new PickerResult()),
                },
                FavoriteSettings =
                {
                    new OldFavoriteSetting("Sample", new PickerSettings()),
                },
            };
            var ss = GenericContractSerializer.Serialize(s);

            var oldState = PickerState.LoadDefault();
            if (oldState != null)
            {
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!isNew)
            {
                // We're coming back from being paused
                // so we don't need to load anything
            }
            else
            {
                var instance = MainViewModel.Instance;
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            isNew = false;
            MainViewModel.Instance.Save();
            mtiks.Instance.Stop();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            MainViewModel.Instance.Save();
            mtiks.Instance.Stop();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

            mtiks.Instance.AddException(e.ExceptionObject);
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            //AdManager.Initialize(AdApplicationId, "10016484", "10016485", "10016486", "10016482");
            mtiks.Instance.Start(MtiksApplicationId, Assembly.GetExecutingAssembly());

            // Increment the launch count and save it back
            Int32 appLaunchCount = 0;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("AppLaunchCount", out appLaunchCount);
            appLaunchCount++;
            IsolatedStorageSettings.ApplicationSettings["AppLaunchCount"] = appLaunchCount;

            // Check if we've updated since the last time we ran
            String currentAppVersion = Assembly.GetExecutingAssembly().FullName;
            String appVersion = null;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("AppVersion", out appVersion);

            if (appVersion == null || appVersion != currentAppVersion)
            {
                System.Diagnostics.Debug.WriteLine("New version detected");

                this.IsNewVersion = true;

                // Save the current app version
                IsolatedStorageSettings.ApplicationSettings["AppVersion"] = currentAppVersion;
            }
            
            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}