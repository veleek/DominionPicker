using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using Ben.Dominion.Models;
using Ben.Utilities;
using BugSense;
using BugSense.Core.Model;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Shell;

namespace Ben.Dominion
{
    public partial class App : Application
    {
        public const String AdApplicationId = "a7ac8297-9d02-405b-9dca-4d702cf50997";
        public const String MtiksApplicationId = "166ff6d5917b9569d549eec40";
        public const String BugSenseApiKey = "4d5d8cdd";
        private Boolean isNew;
        private readonly LicenseInformation license = new LicenseInformation();

        /// <summary>
        /// Create a new instance of <see cref="App"/>
        /// </summary>
        public App()
        {
            BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(Current), this.RootFrame, BugSenseApiKey);

            this.isNew = true;
            AppLog.Instance.Log("Launching: " + Assembly.GetExecutingAssembly().FullName);

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                //MetroGridHelper.IsVisible = true;

                // Override the culture for localization testing
                //Strings.Culture = Ben.Dominion.Models.ConfigurationModel.Instance.CurrentCulture;
                //Strings.Culture = new CultureInfo("fr-CA");
            }

            // Standard Silverlight initialization
            this.InitializeComponent();

            // Phone-specific initialization
            this.InitializePhoneApplication();
        }

        public static App Instance
        {
            get { return Application.Current as App; }
        }

        /// <summary>
        ///     Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public Card SelectedCard { get; set; }

        public Boolean IsTrial
        {
            get
            {
                //return true;
                return this.license.IsTrial();
            }
        }

        public bool IsNewVersion { get; set; }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            // Increment the launch count and save it back
            var appLaunchCount = IsolatedStorageSettings.ApplicationSettings.Increment("AppLaunchCount");
            EasyTracker.GetTracker().SendEvent("app", "launch", null, appLaunchCount);
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!this.isNew)
            {
                // We're coming back from being paused
                // so we don't need to load anything
            }

            EasyTracker.GetTracker().SendEvent("app", "activate", null, 0);
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            this.isNew = false;
            MainViewModel.Instance.Save();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            MainViewModel.Instance.Save();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.Initialize();
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (this.phoneApplicationInitialized)
            {
                return;
            }

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            this.RootFrame = new PhoneApplicationFrame();
            this.RootFrame.Navigated += this.CompleteInitializePhoneApplication;

            // Handle navigation failures
            this.RootFrame.NavigationFailed += this.RootFrame_NavigationFailed;

            //AdManager.Initialize(AdApplicationId, "10016484", "10016485", "10016486", "10016482");

            // Make sure the configuration has been initialized first
            var config = ConfigurationModel.Instance;

            // Then load the main view model
            this.Resources.Add("MainView", MainViewModel.Instance);

            // Check if we've updated since the last time we ran
            var currentAppVersion = Assembly.GetExecutingAssembly().FullName;
            var appVersion = IsolatedStorageSettings.ApplicationSettings.Replace("AppVersion", currentAppVersion);

            if (appVersion == null || appVersion != currentAppVersion)
            {
                AppLog.Instance.Log("New version detected.  Original: {0}, Current: {1}", appVersion, currentAppVersion);
                this.IsNewVersion = true;
            }

            // Ensure we don't initialize again
            this.phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (this.RootVisual != this.RootFrame)
            {
                this.RootVisual = this.RootFrame;
            }

            // Remove this handler since it is no longer needed
            this.RootFrame.Navigated -= this.CompleteInitializePhoneApplication;
        }

        #endregion
    }
}
