using Ben.Utilities;
using System;
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
            //if (instance != null)
            //{
            //    throw new ArgumentException("We should never create more than one instance of this class");
            //}

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
            using (Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream("./Resources/DefaultPickerView.xml"))
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

                // Save the sort order from the current result if there is one.
                ResultSortOrder sortOrder = this.Result?.SortOrder ?? ResultSortOrder.Name;
                this.Result = this.Picker.GenerateCardList(this.Settings, sortOrder);
            }
            finally
            {
                this.IsGenerating = false;
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
                AppLog.Instance.Log("Loading picker view...");
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
                                    StreamReader reader = new StreamReader(stream);
                                    var content = reader.ReadToEnd();
                                    stream.Position = 0;

                                    XmlSerializer serializer = new XmlSerializer(typeof (MainViewModel));

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
                AppLog.Instance.Error("IsolatedStorageException while loading Picker view model.");
                AppLog.Instance.Error(ise.ToString());
                // Just ignore the exception
            }
            catch (SerializationException se)
            {
                AppLog.Instance.Error("Unable to deserialize saved state");
                AppLog.Instance.Error(se.ToString());
            }
            catch (InvalidOperationException ioe)
            {
                AppLog.Instance.Error("MainViewModel was likely empty");
                AppLog.Instance.Error(ioe.ToString());
            }

            if (view == null)
            {
                AppLog.Instance.Log("Using default picker view model");
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
                AppLog.Instance.Log("Saving picker state...");
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
                AppLog.Instance.Error("Unable to save picker state.", e);
            }
        }

        public void Reset()
        {
            Settings = new SettingsViewModel();
        }

        internal void ClearSavedState()
        {
            AppLog.Instance.Log("Clearing out all saved state");
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
