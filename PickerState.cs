using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Ben.Utilities;
using Microsoft.Phone.Shell;
using System.Windows.Threading;

namespace Ben.Dominion
{
    public class PickerState : NotifyPropertyChangedBase
    {
        #region Statics
        public static readonly String PickerStateFileName = "PickerState.xml";

        public static Boolean Loaded = false;
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
            catch(Exception)
            {
                // There was a problem loading the picker state, just ignore
                // this and we'll create a new one.
                //MessageBox.Show("Unable to load picker state: " + e.Message);
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
                    if (UseIsolatedStorage)
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
                    com.mtiks.winmobile.mtiks.Instance.AddException(new Exception("Unable to save picker state", e));
                }
            }

            TimeSpan elapsedTime = DateTime.UtcNow - start;
            AppLog.Instance.Log(String.Format("Loaded State: {0}ms", elapsedTime.TotalMilliseconds));
        }

        /// <summary>
        /// Deletes all saved state from the disk and creates a new one
        /// </summary>
        public static void ResetState()
        {
            ClearSavedState();

            // Will force the creation of a new state
            current = new PickerState();
        }

        /// <summary>
        /// Deletes all saved state from the disk
        /// </summary>
        public static void ClearSavedState()
        {
            try
            {
                // Delete the persisted state
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(PickerStateFileName))
                    {
                        store.DeleteFile(PickerStateFileName);
                    }
                }
            }
            catch { }
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

        private PickerResult result;
        /// <summary>
        /// The most recent picker results
        /// </summary>
        public PickerResult Result
        {
            get { return result; }
            set
            {
                if (value != result)
                {
                    result = value;
                    NotifyPropertyChanged("Result");
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

        /// <summary>
        /// Generates a new card list using the current settings  
        /// </summary>
        /// <returns>True if the list was successfully generated, false otherwise</returns>
        public bool GenerateCardList()
        {
            if (CurrentSettings.SelectedSets.Count == 0)
            {
                MessageBox.Show("Select at least one expansion set to choose from");
                return false;
            }
            this.Result = picker.GenerateCardList();
            this.sortedAlphabetically = true;

            return this.result != null;
        }

        public void ReplaceCard(Card c)
        {
            if (c == null)
            {
                return;
            }

            Int32 index = Result.Cards.IndexOf(c);
            Result.Cards.Remove(c);

            Result.Cards.Insert(index, picker.GetRandomCardOtherThan(Result.Cards));
        }

        private Boolean sortedAlphabetically;
        public void SwapSort()
        {
            if (sortedAlphabetically)
            {
                Result.Cards = Result.Cards.OrderBy(c => c.Cost).ToObservableCollection();
            }
            else
            {
                Result.Cards = Result.Cards.OrderBy(c => c.Name).ToObservableCollection();
            }

            sortedAlphabetically = !sortedAlphabetically;
        }

        /// <summary>
        /// This will cancel any current card list generation
        /// </summary>
        public void CancelGeneration()
        {
            picker.CancelGeneration();
        }
    }
}
