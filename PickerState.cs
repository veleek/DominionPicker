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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Ben.Dominion
{
    public class PickerState : NotifyPropertyChangedBase
    {
        #region Statics
        public static readonly String PickerStateFileName = "PickerState.xml";

        //private static Boolean Loaded = false;
        private static Boolean UseIsolatedStorage = true;

        public static PickerState Default
        {
            get
            {
                PickerState state = null;
                try
                {
                    using (var stream = Microsoft.Xna.Framework.TitleContainer.OpenStream("./Assets/DefaultPickerState.xml"))
                    {
                        state = GenericXmlSerializer.Deserialize<PickerState>(stream);
                    }
                }
                finally
                {
                    if (state == null)
                    {
                        state = new PickerState();
                    }
                }

                return state;
            }
        }

        private static PickerState current;
        public static PickerState Current
        {
            get
            {
                if (current == null)
                {
                    //throw new InvalidOperationException();
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
                Debug.WriteLine("Loading picker state...");
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
            catch(IsolatedStorageException)
            {
                Debug.WriteLine("IsolatedStorageException while loading Picker State.");
                // Just ignore the exception
            }

            if (state == null)
            {
                Debug.WriteLine("Using default picker state");
                state = PickerState.Default;
            }
            else
            {
                // TODO: This is a bit of a hack to enable 'updating' to a newer version of the 
                // settings which will include all of sets so they don't have to reset their settings
                state.CurrentSettings = state.CurrentSettings.Clone();
            }

            TimeSpan elapsedTime = DateTime.UtcNow - start;
            AppLog.Instance.Log(String.Format("Loaded State: {0}ms", elapsedTime.TotalMilliseconds));
            current = state;
        }

        public static void Save()
        {
            if (current != null)
            {
                try
                {
                    Debug.WriteLine("Saving picker state...");
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
                catch(IsolatedStorageException e)
                {
                    Debug.WriteLine("There was an issue trying to save the picker state.");
                    com.mtiks.winmobile.mtiks.Instance.AddException(new IOException("Unable to save picker state", e));
                }
            }
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
            catch (IsolatedStorageException) { }

            // The clear out the current state, it will get reloaded when it needs to.
            current = null;
        }

        #endregion Statics

        public Picker picker { get; set; }

        private bool isGenerating;
        public bool IsGenerating
        {
            get
            {
                return isGenerating;
            }
            set
            {
                if (value != isGenerating)
                {
                    isGenerating = value;
                    NotifyPropertyChanged("IsGenerating");
                }
            }
        }

        private PickerSettings currentSettings;
        public PickerSettings CurrentSettings
        {
            get 
            {
                if (currentSettings == null)
                {
                    currentSettings = PickerSettings.DefaultSettings;
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
        /// <remarks>
        /// Observable collection implements INotifyPropertyChanged, and we shouldn't be 
        /// actually changing the collection instances in this class, so don't bother with 
        /// NotifyPropertyChanged stuff on this class.
        /// </remarks>
        public ObservableCollection<FavoriteSetting> FavoriteSettings { get; set; }

        /// <summary>
        /// A saved list of favorite sets
        /// </summary>
        /// <remarks>
        /// Observable collection implements INotifyPropertyChanged, and we shouldn't be 
        /// actually changing the collection instances in this class, so don't bother with 
        /// NotifyPropertyChanged stuff on this class.
        /// </remarks>
        public ObservableCollection<FavoriteSet> FavoriteSets { get; set; }

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

        public ResultSortOrder SortOrder { get; set; }

        public int PivotIndex { get; set; }

        public PickerState()
        {
            //this.Favorites = new ObservableCollection<PickerFavorite>();
            this.FavoriteSettings = new ObservableCollection<FavoriteSetting>();
            this.FavoriteSets = new ObservableCollection<FavoriteSet>();

            this.picker = new Picker();
            this.SortOrder = ResultSortOrder.Name;
        }

        public void SaveFavoriteSettings(String name)
        {
            FavoriteSettings.Add(new FavoriteSetting(name, CurrentSettings.Clone()));
        }

        public void SaveFavoriteCardSet(String name)
        {
            FavoriteSets.Add(new FavoriteSet(name, this.Result));
        }

        public void LoadSettings(PickerSettings settings)
        {
            CurrentSettings = settings.Clone();
        }

        public void Reset()
        {
            CurrentSettings = PickerSettings.DefaultSettings;
        }

        /// <summary>
        /// Generates a new card list using the current settings  
        /// </summary>
        /// <returns>True if the list was successfully generated, false otherwise</returns>
        public bool GenerateCardList()
        {
            if (CurrentSettings.SelectedSets.Count == 0)
            {
                return false;
            }

            try
            {
                IsGenerating = true;

                this.Result = picker.GenerateCardList();
                this.SortBy(SortOrder);
            }
            finally
            {
                IsGenerating = false;
            }

            return this.result != null;
        }

        public void ReplaceCard(Card c)
        {
            if (c == null)
            {
                return;
            }

            int index = -1;
            for (int i = 0; i < Result.Cards.Count; i++)
            {
                if (Result.Cards[i].Name == c.Name)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                System.Diagnostics.Debugger.Break();
            Result.Cards.RemoveAt(index);
            Result.Cards.Insert(index, picker.GetRandomCardOtherThan(Result.Cards));
        }

        public void SortBy(ResultSortOrder sortOrder)
        {
            if (Result != null && Result.Cards != null)
            {
                switch (sortOrder)
                {
                    case ResultSortOrder.Name:
                        Result.Cards = Result.Cards.OrderBy(c => c.Name).ToObservableCollection();
                        break;
                    case ResultSortOrder.Cost:
                        Result.Cards = Result.Cards.OrderBy(c => c.Cost).ToObservableCollection();
                        break;
                    case ResultSortOrder.Set:
                        // Order them by set then alphabetically
                        // This makes the most sense because it's probably 
                        // being used by someone who has the cards in the 
                        // original set boxes
                        Result.Cards = Result.Cards.OrderBy(c => c.Set + "," + c.Name).ToObservableCollection();
                        break;
                    default:
                        // Orders them randomly, not used
                        Result.Cards = Result.Cards.OrderBy(c => Guid.NewGuid()).ToObservableCollection();
                        break;
                }
            }

            SortOrder = sortOrder;
        }

        /// <summary>
        /// This will cancel any current card list generation
        /// </summary>
        public void CancelGeneration()
        {
            picker.CancelGeneration();
        }
    }

    public enum ResultSortOrder
    {
        None,
        Name,
        Cost,
        Set,
    }
}
