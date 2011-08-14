using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Advertising.Mobile.UI;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Ben.Phone
{
    public class AdManager
    {
        private static Random rand;
        private static DispatcherTimer timer;

        public static AdControl Ad { get; private set; }
        public static String ApplicationId
        {
            get { return Ad.ApplicationId; }
            private set { Ad.ApplicationId = value; }
        }
        public static List<String> AdUnits { get; private set; }
        public static Double CycleSeconds 
        {
            get { return timer.Interval.TotalSeconds; }
            set { timer.Interval = TimeSpan.FromSeconds(value); }
        }

        static AdManager()
        {
            #if DEBUG
            AdControl.TestMode = true;
            #endif

            rand = new Random();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);           

            CycleSeconds = 30;
            Ad = new AdControl();
            Ad.RotationEnabled = false;
            Ad.AdModel = AdModel.Contextual;
            Ad.Width = 480;
            Ad.Height = 80;
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            Ad.RequestNextAd();   
        }

        public static void Initialize(String applicationId, params String[] adUnits)
        {
            #if DEBUG
            applicationId = "test_client";
            #endif

            // Set the application id on the AdControl
            ApplicationId = applicationId;

            // If no ad units were provided, just use the test ones
            if(adUnits == null || adUnits.Length == 0)
            {
                adUnits = new String[] { "Image480_80", "TextAd", "TextAd" };
            }
            AdUnits = adUnits.ToList();

            // The first add unit is considered the default, the remaining ad units 
            // are selected at ~<10% of the rate of the default.  
            Int32 adUnitIndex = rand.Next(-9 * (AdUnits.Count - 1), AdUnits.Count);
            if (adUnitIndex < 0)
            {
                adUnitIndex = 0;
            }

            // Then set the AdUnitId on the control
            Ad.AdUnitId = AdUnits[adUnitIndex];
        }

        public static void LoadAd(Panel container)
        {
            // If the container doesn't has an ad control
            if (container.Children.Any(c => c is AdControl))
            {
                // Remove any ads if there are any
                UnloadAd(container);
            }

            container.Children.Add(Ad);
        }

        public static void UnloadAd(Panel container)
        {
            foreach (AdControl adControl in container.Children.OfType<AdControl>().ToList())
            {
                container.Children.Remove(adControl);
            }
        }

        public static void StartCycling()
        {
            timer.Start();
        }

        public static void StopCycling()
        {
            timer.Stop();
        }
    }
}
