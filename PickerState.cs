using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using Microsoft.Phone.Shell;
using System.Text;
using System.Xml;

namespace Ben.Dominion
{
    public class PickerState : INotifyPropertyChanged
    {
        #region Statics
        public static readonly String PickerStateFileName = "PickerState.xml";
        public static readonly String PickerStateName = "PickerState";

        public static Boolean Loaded = false;
        public static Boolean UseApplicationService = false;
        public static Boolean UseIsolatedStorage = true;

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
            DateTime start = DateTime.UtcNow;
            try
            {
                if (UseApplicationService)
                {
                    Object obj = null;
                    if (PhoneApplicationService.Current.State.TryGetValue(PickerStateName, out obj))
                    {
                        state = obj as PickerState;
                    }
                }

                if (UseIsolatedStorage)
                {
                    if (state == null)
                    {
                        using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (store.FileExists(PickerStateFileName))
                            {
                                using (Stream stream = store.OpenFile(PickerStateFileName, FileMode.Open))
                                {
                                    state = GenericContractSerializer.Deserialize<PickerState>(stream);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Unable to load picker state: " + e.Message);
            }

            if (state == null)
            {
                state = new PickerState();
            }

            TimeSpan elapsedTime = DateTime.UtcNow - start;
            AppLog.Instance.Log(String.Format("Loaded State: {0}ms", elapsedTime.TotalMilliseconds));
            current = state;
            Loaded = true;
        }

        public static void Save()
        {
            DateTime start = DateTime.UtcNow;

            if (current != null)
            {
                try
                {
                    if (UseApplicationService)
                    {
                        PhoneApplicationService.Current.State[PickerStateName] = current;
                    }
                    else if (UseIsolatedStorage)
                    {
                        using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            using (Stream stream = store.OpenFile(PickerStateFileName, FileMode.Create))
                            {
                                GenericContractSerializer.Serialize(stream, current);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show("Unable to save picker state: " + e.Message);
                }
            }

            TimeSpan elapsedTime = DateTime.UtcNow - start;
            AppLog.Instance.Log(String.Format("Loaded State: {0}ms", elapsedTime.TotalMilliseconds));
        }

        public static void ResetState()
        {
            try
            {
                // Delete the application state 
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove(PickerStateName);

                // And the persisted state
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(PickerStateFileName))
                    {
                        store.DeleteFile(PickerStateFileName);
                    }
                }
            }
            catch { }

            // Will force the creation of a new state
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

        public void SaveFavorite(String name)
        {
            PickerSettings fav = CurrentSettings.Clone();
            fav.Name = name;

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
            if (CurrentSettings.SelectedSets.Count == 0)
            {
                MessageBox.Show("Selected at least one set to choose from");
                return;
            }
            this.CardList = picker.GenerateCardList();
            this.sortedAlphabetically = true;
        }

        public void ReplaceCard(Card c)
        {
            if (c == null)
            {
                return;
            }

            Int32 index = CardList.IndexOf(c);
            CardList.Remove(c);

            CardList.Insert(index, picker.GetRandomCardOtherThan(CardList));
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

        private Boolean sortedAlphabetically;
        public void SwapSort()
        {
            if (sortedAlphabetically)
            {
                CardList = CardList.OrderBy(c => c.Cost).ToObservableCollection();
            }
            else
            {
                CardList = CardList.OrderBy(c => c.Name).ToObservableCollection();
            }

            sortedAlphabetically = !sortedAlphabetically;
        }
    }
}
