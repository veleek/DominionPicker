using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Phone.Management.Deployment;
using Windows.System;
using Windows.UI.Xaml;

namespace DominionPickerLauncher
{
    public static class PickerLauncher
    {
        public static void CheckAndLaunch()
        {
            if (PickerInstalled())
            {
                TryLaunch(true);
            }
        }

        /// <summary>
        /// Checks to see if Dominion Picker is installed
        /// </summary>
        /// <returns></returns>
        public static bool PickerInstalled()
        {
            return InstallationManager.FindPackagesForCurrentPublisher().Any(p => p.Id.Name == "Dominion Picker");
        }


        public static bool TryLaunch(bool exitIfSuccessful)
        {
            // Then just launch it using our registered URI
            var launcherTask = Launcher.LaunchUriAsync(new Uri("dominionpicker:DominionPickerLauncher")).AsTask();
            launcherTask.Wait();

            if (launcherTask.Status == TaskStatus.RanToCompletion)
            {
                if (exitIfSuccessful)
                {
                    // We successfully launched the app
                    Application.Current.Exit();
                }

                return true;
            }

            return false;
        }
    }
}