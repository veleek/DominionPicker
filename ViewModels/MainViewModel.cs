using Ben.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ben.Dominion.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private SettingsViewModel settings = new SettingsViewModel();

        public MainViewModel()
        {
            string xml = GenericXmlSerializer.Serialize(settings);
        }

        public SettingsViewModel Settings
        {
            get { return settings; }
            set
            {
                SetProperty(ref settings, value, "Settings");
            }
        }
    }
}
