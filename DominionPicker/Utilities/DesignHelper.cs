using System;
using System.IO.IsolatedStorage;

namespace Ben.Utilities
{
    public class DesignHelper
    {
        private static bool? isInDesignMode = null;
        public static bool IsInDesignMode
        {
            get
            {
                if (!isInDesignMode.HasValue)
                {
                    try
                    {
                        var randomSetting = IsolatedStorageSettings.ApplicationSettings.Contains("_____________");
                        isInDesignMode = false;

                    }
                    catch (Exception)
                    {
                        isInDesignMode = true;
                    }
                }

                return isInDesignMode.Value;
            }
        }
    }
}
