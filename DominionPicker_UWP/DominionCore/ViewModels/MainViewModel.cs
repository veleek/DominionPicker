using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Ben.Utilities;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Ben.Dominion.ViewModels
{
    public class MainViewModel
       : NotifyPropertyChangedBase
    {
        public static readonly String PickerStateFileName = @"PickerView.xml";
        private static AsyncLazy<MainViewModel> lazyInstance = new AsyncLazy<MainViewModel>((Func<Task<MainViewModel>>)LoadAsync);
        private static Boolean UseIsolatedStorage = true;
        private SettingsViewModel settings = new SettingsViewModel();
        private PickerResult result = new PickerResult();
        private FavoritesViewModel favorites = new FavoritesViewModel();
        private BlackMarketViewModel blackMarket = new BlackMarketViewModel();
        private bool isGenerating;

        public MainViewModel()
        {
        }

        public static MainViewModel Instance
        {
            get
            {
                return lazyInstance.Value.GetResult();
            }
        }

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
            get
            {
                return this.blackMarket;
            }
            set
            {
                this.SetProperty(ref this.blackMarket, value, "BlackMarket");
            }
        }

        public ConfigurationModel Configuration
        {
            get
            {
                return ConfigurationModel.Instance;
            }
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
        public static async Task<MainViewModel> LoadDefault()
        {
            try
            {
                StorageFolder packageLocation = Package.Current.InstalledLocation;
                using (Stream stream = await packageLocation.SafeOpenStreamForReadAsync(@"DominionCore\Resources\DefaultPickerView.xml"))
                {
                    return GenericXmlSerializer.Deserialize<MainViewModel>(stream);
                }
            }
            catch (Exception e)
            {
                AppLog.Instance.Log("Unable to load view model:" + e.Message);
                throw;
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
                ResultSortOrder sortOrder = this.Result != null ? this.Result.SortOrder : ResultSortOrder.Name;
                this.Result = Picker.GenerateCardList(this.Settings, sortOrder);
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
            Task cancelGenerationTask = Picker.CancelGeneration();
        }

        public void SaveFavoriteSettings(String name)
        {
            Favorites.FavoriteSettings.Add(new FavoriteSetting(name, Settings));
        }

        public void SaveFavoriteCardSet(String name)
        {
            Favorites.FavoriteSets.Add(new FavoriteSet(name, this.Result));
        }

        public static async Task<MainViewModel> LoadAsync()
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
                        StorageFolder folder = ApplicationData.Current.LocalFolder;
                        IStorageFile file = await folder.TryGetItemAsync(PickerStateFileName) as IStorageFile;

                        if (file != null)
                        {
                            using (Stream stream = await file.OpenStreamForReadAsync())
                            {
                                StreamReader reader = new StreamReader(stream);
                                var content = reader.ReadToEnd();
                                stream.Position = 0;
                                XmlSerializer serializer = new XmlSerializer(typeof(MainViewModel));
                                view = serializer.Deserialize(stream) as MainViewModel;
                                //view = GenericXmlSerializer.Deserialize<MainViewModel>(stream);
                                //view = GenericContractSerializer.Deserialize<MainViewModel>(stream);
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
            catch (Exception)
            {
                throw;
            }

            if (view == null)
            {
                AppLog.Instance.Log("Using default picker view model");
                view = await LoadDefault();
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

            lazyInstance = new AsyncLazy<MainViewModel>((Func<Task<MainViewModel>>)LoadAsync);
        }

    }
}