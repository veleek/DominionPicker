//using GoogleAnalytics;
using GalaSoft.MvvmLight.Threading;
//using BugSense.Core.Model;
//using BugSense;
using Ben.Utilities;
using Ben.Dominion.Views;
using Ben.Dominion.Models;
using System.Reflection;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.ApplicationInsights;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409
namespace Ben.Dominion
{

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
       : Application
    {
        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        public TelemetryClient TelemetryClient = new TelemetryClient();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            //NavigationServiceHelper.RegisterAll<PickerView>();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Application_Launching(e);
            Frame rootFrame = Window.Current.Content as Frame;
            if (!TryToNavigateToTileReference(rootFrame, e))
            {
                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();
                    // Set the default language
                    rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }
                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(Ben.Dominion.MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
            //[WP8SL_TO_UWP] The following code was added to emulate the default behavior of
            // the back button on WP8SL
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (object sender, Windows.UI.Core.BackRequestedEventArgs backEventArgs) =>
               {
                   if (!backEventArgs.Handled && rootFrame.CanGoBack)
                   {
                       rootFrame.GoBack();
                       backEventArgs.Handled = true;
                   }
               };
            bool firstVisibilityEvent = true;
            Window.Current.CoreWindow.VisibilityChanged += (windowObj, firstVisibilityEventArgs) =>
               {
                   if (firstVisibilityEvent)
                   {
                       firstVisibilityEvent = false;
                   }
                   else
                   {
                       if (firstVisibilityEventArgs.Visible)
                       {
                           Application_Activated(null, null);
                        // Refresh the current page
                        Frame currentFrame = Window.Current.Content as Frame;
                           if (currentFrame != null)
                           {
                               var navstate = currentFrame.GetNavigationState();
                               currentFrame.SetNavigationState(navstate);
                           }
                       }
                       else
                       {
                           Application_Deactivated(null, null);
                       }
                   }
               };
        }

        bool TryToNavigateToTileReference(Frame rootFrame, LaunchActivatedEventArgs e)
        {
            switch (e.TileId)
            {
                case "_Views_AboutPage.xaml":
                    rootFrame.Navigate(typeof(AboutPage), e.Arguments);
                    return true;
                case "_Views_BlackMarketPage.xaml":
                    rootFrame.Navigate(typeof(BlackMarketPage), e.Arguments);
                    return true;
                case "_Views_CardFilterPage.xaml":
                    rootFrame.Navigate(typeof(CardFilterPage), e.Arguments);
                    return true;
                case "_Views_ConfigurationPage.xaml":
                    rootFrame.Navigate(typeof(ConfigurationPage), e.Arguments);
                    return true;
                case "_Views_CardInfo.xaml":
                    rootFrame.Navigate(typeof(CardInfo), e.Arguments);
                    return true;
                //case "_Views_DebugPage.xaml":
                //   rootFrame.Navigate(typeof(DebugPage), e.Arguments);
                //   return true;
                //case "_Views_ImportExportPage.xaml":
                //   rootFrame.Navigate(typeof(ImportExportPage), e.Arguments);
                //   return true;
                case "_Views_MainPage.xaml":
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    return true;
                case "_Views_ResultsViewer.xaml":
                    rootFrame.Navigate(typeof(ResultsViewer), e.Arguments);
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
            Application_Deactivated(sender, e);
            Application_Closing(sender, e);
        }

        public const String AdApplicationId = "a7ac8297-9d02-405b-9dca-4d702cf50997";
        public const String MtiksApplicationId = "166ff6d5917b9569d549eec40";
        public const String BugSenseApiKey = "4d5d8cdd";
        private Boolean isNew;
        private readonly Windows.ApplicationModel.Store.LicenseInformation license = Windows.ApplicationModel.Store.CurrentApp.LicenseInformation;

        public static App Instance
        {
            get
            {
                return Current as App;
            }
        }

        public Card SelectedCard { get; set; }

        public bool IsNewVersion { get; set; }

        private async void Application_Launching(LaunchActivatedEventArgs args)
        {
            // Increment the launch count and save it back
            var appLaunchCount = Windows.Storage.ApplicationData.Current.LocalSettings.Increment("AppLaunchCount");
            //EasyTracker.GetTracker().SendEvent("app", "launch", null, appLaunchCount);
            try
            {
                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///DominionVCD.xml")));
            }
            catch (Exception)
            {
                // If the user has not accepted the Speech Privacy Policy, then this 
                // will throw an exception, but we can ignore it.  Just log a note of it.
                //EasyTracker.GetTracker().SendEvent("voice", "disabled", null, 0);
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, System.Object e)
        {
            if (!this.isNew)
            {
                // We're coming back from being paused
                // so we don't need to load anything
            }
            //EasyTracker.GetTracker().SendEvent("app", "activate", null, 0);
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, System.Object e)
        {
            this.isNew = false;
            MainViewModel.Instance.Save();
        }

        private void Application_Closing(object obj, SuspendingEventArgs args)
        {
            MainViewModel.Instance.Save();
        }

    }

}