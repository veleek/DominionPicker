using Ben.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        public static readonly String PickerStateFileName = "PickerView.xml";

        private static MainViewModel instance;
        private static Boolean UseIsolatedStorage = true;

        private SettingsViewModel settings = new SettingsViewModel();
        private PickerResult result = new PickerResult();
        private FavoritesViewModel favorites = new FavoritesViewModel();
        private BlackMarketViewModel blackMarket = new BlackMarketViewModel();

        private bool isGenerating;

        public MainViewModel()
        {
            if (instance != null)
            {
                throw new ArgumentException("We should never create more than one instance of this class");
            }

            Picker = new Picker();
        }

        public static MainViewModel Instance
        {
            get
            {
                return instance ?? (instance = Load());
            }
        }

        [XmlIgnore]
        public Picker Picker { get; set; }

        public SettingsViewModel Settings
        {
            get
            {
                return settings;
            }

            set
            {
                SetProperty(ref settings, value, "Settings");
            }
        }

        public PickerResult Result
        {
            get
            {
                return result;
            }

            set
            {
                SetProperty(ref result, value, "Result");
            }
        }

        public FavoritesViewModel Favorites
        {
            get
            {
                return favorites;
            }

            set
            {
                SetProperty(ref favorites, value, "Favorites");
            }
        }

        public BlackMarketViewModel BlackMarket
        {
            get { return this.blackMarket; }
            set { this.SetProperty(ref this.blackMarket, value, "BlackMarket"); }
        }

        [XmlIgnore]
        public bool IsGenerating
        {
            get
            {
                return isGenerating;
            }
            set
            {
                SetProperty(ref isGenerating, value, "IsGenerating");
            }
        }

        public int PivotIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static MainViewModel LoadDefault()
        {
            using (Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream("./Assets/DefaultPickerState_1.8.xml"))
            {
                return GenericXmlSerializer.Deserialize<MainViewModel>(stream);
            }
        }

        /// <summary>
        /// Generates a new card list using the current settings  
        /// </summary>
        /// <returns>True if the list was successfully generated, false otherwise</returns>
        public bool GenerateCardList()
        {
            if (this.Settings.SelectedSets.Count == 0)
            {
                return false;
            }

            try
            {
                this.IsGenerating = true;

                var result = Picker.GenerateCardList(this.Settings);
                // Save the current sort order
                result.Sort(this.Result != null ? this.Result.SortOrder : ResultSortOrder.Name);
                this.Result = result;
            }
            finally
            {
                IsGenerating = false;
            }

            return this.Result != null;
        }

        /// <summary>
        /// This will cancel any current card list generation
        /// </summary>
        public void CancelGeneration()
        {
            Picker.CancelGeneration();
        }

        public void SaveFavoriteSettings(String name)
        {
            Favorites.FavoriteSettings.Add(new FavoriteSetting(name, Settings));
        }

        public void SaveFavoriteCardSet(String name)
        {
            Favorites.FavoriteSets.Add(new FavoriteSet(name, this.Result));
        }
        
        public static MainViewModel Load()
        {
            MainViewModel view = null;
            DateTime start = DateTime.UtcNow;
            try
            {
                Debug.WriteLine("Loading picker view...");
                if (UseIsolatedStorage)
                {
                    if (view == null)
                    {
                        using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (store.FileExists(PickerStateFileName))
                            {
                                using (Stream stream = store.OpenFile(PickerStateFileName, FileMode.Open))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(MainViewModel));

                                    view = serializer.Deserialize(stream) as MainViewModel;

                                    //view = GenericXmlSerializer.Deserialize<MainViewModel>(stream);
                                    //view = GenericContractSerializer.Deserialize<MainViewModel>(stream);
                                }
                            }
                        }
                    }
                }
            }
            catch (IsolatedStorageException ise)
            {
                Debug.WriteLine("IsolatedStorageException while loading Picker view model.");
                Debug.WriteLine(ise.ToString());
                // Just ignore the exception
            }
            catch (SerializationException se)
            {
                Debug.WriteLine("Unable to deserialize saved state");
                Debug.WriteLine(se.ToString());
            }

            if (view == null)
            {
                Debug.WriteLine("Using default picker view model");
                view = LoadDefault();
            }
            else
            {
                // TODO: This is a bit of a hack to enable 'updating' to a newer version of the 
                // settings which will include all of sets so they don't have to reset their settings
                //throw new NotImplementedException();
            }

            TimeSpan elapsedTime = DateTime.UtcNow - start;
            AppLog.Instance.Log(String.Format("Loaded view: {0}ms", elapsedTime.TotalMilliseconds));
            return view;
        }

        public void Save()
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
                            GenericXmlSerializer.Serialize(stream, this);
                            //GenericContractSerializer.Serialize(stream, this);
                        }
                    }
                }
            }
            catch (IsolatedStorageException e)
            {
                Debug.WriteLine("There was an issue trying to save the picker state.");
                com.mtiks.winmobile.mtiks.Instance.AddException(new IOException("Unable to save picker state", e));
            }
        }

        public void Reset()
        {
            Settings = new SettingsViewModel();
        }

        internal void ClearSavedState()
        {
            Debug.WriteLine("Clearing out all saved state");
            if (UseIsolatedStorage)
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(PickerStateFileName))
                    {
                        store.DeleteFile(PickerStateFileName);
                    }
                }
            }

            instance = null;
        }
    }
}
