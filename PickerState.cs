using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Windows;

namespace Ben.Dominion
{
    public class PickerState : INotifyPropertyChanged
    {
        #region Statics
        public static readonly String PickerStateFileName = "PickerState.xml";

        public static Boolean Loaded = false;

        private static PickerState current;
        public static PickerState Current
        {
            get
            {
                if (current == null)
                {
                    Load();
                }

                return current;
            }
        }

        public static void Load()
        {
            PickerState state = null;

            try
            {
                Object obj = null;
                if (Microsoft.Phone.Shell.PhoneApplicationService.Current.State.TryGetValue("State", out obj))
                {
                    state = obj as PickerState;
                }

                if (state == null)
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(PickerStateFileName))
                        {
                            using (Stream stream = store.OpenFile(PickerStateFileName, FileMode.Open))
                            {
                                state = GenericSerializer.Deserialize<PickerState>(stream);
                            }
                        }
                    }
                }
            }
            catch
            { }

            if (state == null)
            {
                state = new PickerState();
            }

            Loaded = true;
            current = state;
        }

        public static void Save()
        {
            Current.SaveState();
        }

        public static void ResetState()
        {
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("State");
            current = new PickerState();
        }
        #endregion

        private Picker picker { get; set; }

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
            set
            {
                if (value != currentSettings)
                {
                    currentSettings = value;
                    NotifyPropertyChanged("CurrentSettings");
                }
            }
        }

        /// <summary>
        /// A saved list of favorite settings
        /// </summary>
        public ObservableCollection<PickerSettings> FavoriteSettings { get; set; }

        private ObservableCollection<Card> cardList;
        /// <summary>
        /// The current resultant card list
        /// </summary>
        public ObservableCollection<Card> CardList
        {
            get
            {
                return cardList;
            }
            set
            {
                if (value != cardList)
                {
                    cardList = value;
                    NotifyPropertyChanged("CardList");
                }
            }
        }

        public PickerState()
        {
            this.FavoriteSettings = new ObservableCollection<PickerSettings>();
            this.picker = new Picker();
        }

        public void SaveState()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (Stream stream = store.OpenFile(PickerStateFileName, FileMode.Create))
                {
                    GenericSerializer.Serialize(stream, this);
                }
            }
        }

        public void SaveFavorite()
        {
            PickerSettings fav = CurrentSettings.Clone();
            fav.Name = fav.SelectedSets.Select(s => s.ToString()).Aggregate((a, b) => a + ", " + b);

            FavoriteSettings.Add(fav);
        }

        public void LoadSettings(Int32 favoriteIndex)
        {
            LoadSettings(FavoriteSettings[favoriteIndex]);
        }

        public void LoadSettings(PickerSettings settings)
        {
            CurrentSettings = settings.Clone();
        }

        public void Reset()
        {
            CurrentSettings = new PickerSettings();
        }

        public void GenerateCardList()
        {
            this.CardList = picker.GenerateCardList();
        }

        public void ReplaceCard(Card c)
        {
            if (c == null)
            {
                return;
            }

            Int32 index = CardList.IndexOf(c);
            CardList.Remove(c);

            CardList.Insert(index, picker.GetRandomCard(CardList));
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
