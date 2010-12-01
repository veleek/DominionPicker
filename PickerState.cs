using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public class PickerState : INotifyPropertyChanged
    {
        public static readonly String PickerStateFileName = "PickerState.xml";

        public static Boolean Loaded = false;

        private static PickerState current;
        public static PickerState Current
        {
            get
            {
                if (current == null)
                {
                    current = Load();
                }

                return current;
            }
        }

        public static PickerState Load()
        {
            PickerState state = null;

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(PickerStateFileName))
                {
                    using (Stream stream = store.OpenFile(PickerStateFileName, FileMode.Open))
                    {
                        state = GenericSerializer.Deserialize<PickerState>(stream);
                    }
                }
                else
                {
                    state = new PickerState();
                }
            }

            Loaded = true;
            return state;
        }

        private PickerSettings currentSettings;
        public PickerSettings CurrentSettings
        {
            get 
            {
                if (currentSettings == null)
                {
                    currentSettings = new PickerSettings();
                }

                return currentSettings; 
            }
            private set
            {
                if (value != currentSettings)
                {
                    currentSettings = value;
                    NotifyPropertyChanged("CurrentSettings");
                }
            }
        }

        public Picker CurrentPicker { get; set; }

        /// <summary>
        /// A saved list of favorite settings
        /// </summary>
        public ObservableCollection<PickerSettings> FavoriteSettings { get; set; }

        /// <summary>
        /// The current resultant card list
        /// </summary>
        public ObservableCollection<Card> CardList { get; set; }

        public void SaveFavorite()
        {
            FavoriteSettings.Add(CurrentSettings.Clone());
        }

        public void LoadSettings(Int32 favoriteIndex)
        {
            LoadSettings(FavoriteSettings[favoriteIndex]);
        }

        public void LoadSettings(PickerSettings settings)
        {
            CurrentSettings = settings;
        }

        public void Reset()
        {
            CurrentSettings = new PickerSettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
            {
                h(this, e);
            }
        }
    }
}
