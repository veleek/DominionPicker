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
                // Just ignore the exception
            }

            if (state == null)
            {
                state = PickerState.Default;
            }

            TimeSpan elapsedTime = DateTime.UtcNow - start;
            AppLog.Instance.Log(String.Format("Loaded State: {0}ms", elapsedTime.TotalMilliseconds));
            current = state;
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
                catch(IsolatedStorageException e)
                {
                    com.mtiks.winmobile.mtiks.Instance.AddException(new IOException("Unable to save picker state", e));
                }
            }

            TimeSpan elapsedTime = DateTime.UtcNow - start;
            AppLog.Instance.Log(String.Format("Loaded State: {0}ms", elapsedTime.TotalMilliseconds));
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

            // Then reload (or recreate it)
            Load();
        }

        #endregion Statics

        private Picker picker { get; set; }

        public Boolean IsGenerating
        {
            get { return picker.IsGenerating; }
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
        public ObservableCollection<FavoriteSetting> FavoriteSettings { get; set; }
        /// <summary>
        /// A saved list of favorite sets
        /// </summary>
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

        public PickerState()
        {
            //this.Favorites = new ObservableCollection<PickerFavorite>();
            this.FavoriteSettings = new ObservableCollection<FavoriteSetting>();
            this.FavoriteSets = new ObservableCollection<FavoriteSet>();

            this.picker = new Picker();
            this.picker.PropertyChanged += new PropertyChangedEventHandler(picker_PropertyChanged);
            this.SortOrder = ResultSortOrder.Name;
        }

        void picker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsGenerating")
            {
                NotifyPropertyChanged("IsGenerating");
            }
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
            this.Result = picker.GenerateCardList();
            this.SortBy(SortOrder);

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
