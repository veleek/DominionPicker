using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Phone.Management.Deployment;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace DominionPickerLauncher
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            this.Resuming += this.OnResuming;
        }

        private void OnResuming(object sender, object o)
        {
            // Check to see if Dominion Picker is installed
            if (InstallationManager.FindPackagesForCurrentPublisher().Any(p => p.Id.Name == "Dominion Picker"))
            {
                // Then just launch it using our registered URI
                var launcherTask = Launcher.LaunchUriAsync(new Uri("dominionpicker:DominionPickerLauncher")).AsTask();
                launcherTask.Wait();

                if (launcherTask.Status == TaskStatus.RanToCompletion)
                {
                    // We successfully launched the app
                    App.Current.Exit();
                }
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Check to see if Dominion Picker is installed
            if (InstallationManager.FindPackagesForCurrentPublisher().Any(p => p.Id.Name == "Dominion Picker"))
            {
                // Then just launch it using our registered URI
                var launcherTask = Launcher.LaunchUriAsync(new Uri("dominionpicker:DominionPickerLauncher")).AsTask();
                launcherTask.Wait();

                if (launcherTask.Status == TaskStatus.RanToCompletion)
                {
                    // We successfully launched the app
                    App.Current.Exit();
                }
            }

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.ContentTransitions = null;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof (MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }
    }
}