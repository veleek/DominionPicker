using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Serialization;
using Ben.Utilities;

namespace Ben.Dominion
{
    [XmlRoot(Namespace = "")]
    public class PickerState
       : NotifyPropertyChangedBase
    {
        public static readonly String PickerStateFileName = "PickerState.xml";
        //private static Boolean Loaded = false;
        private static Boolean UseIsolatedStorage = true;

        public PickerState()
        {
            //this.Favorites = new ObservableCollection<PickerFavorite>();
            this.FavoriteSettings = new ObservableCollection<OldFavoriteSetting>();
            this.FavoriteSets = new ObservableCollection<OldFavoriteSet>();
        }

        public PickerSettings CurrentSettings { get; set; }

        /// <summary>
        /// A saved list of favorite settings
        /// </summary>
        /// <remarks>
        /// Observable collection implements INotifyPropertyChanged, and we shouldn't be
        /// actually changing the collection instances in this class, so don't bother with
        /// NotifyPropertyChanged stuff on this class.
        /// </remarks>
        public ObservableCollection<OldFavoriteSetting> FavoriteSettings { get; set; }

        /// <summary>
        /// A saved list of favorite sets
        /// </summary>
        /// <remarks>
        /// Observable collection implements INotifyPropertyChanged, and we shouldn't be
        /// actually changing the collection instances in this class, so don't bother with
        /// NotifyPropertyChanged stuff on this class.
        /// </remarks>
        public ObservableCollection<OldFavoriteSet> FavoriteSets { get; set; }

        public static PickerState Load()
        {
            return Load(PickerStateFileName);
        }

        public static PickerState Load(string fileName)
        {
            PickerState state = null;
            try
            {
                if (UseIsolatedStorage)
                {
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(fileName))
                        {
                            AppLog.Instance.Log("Loading picker state...");
                            using (Stream stream = store.OpenFile(fileName, FileMode.Open))
                            {
                                state = GenericContractSerializer.Deserialize<PickerState>(stream);
                            }
                        }
                    }
                }
            }
            catch (IsolatedStorageException)
            {
                AppLog.Instance.Error("IsolatedStorageException while loading Picker State.");
                // Just ignore the exception
            }
            catch (Exception e)
            {
                AppLog.Instance.Error("Unable to load old picker state. " + e);
                // Just ignore the exception
            }
            return state;
        }

        public static async System.Threading.Tasks.Task<PickerState> LoadDefault()
        {
            PickerState state = null;
            try
            {
                AppLog.Instance.Log("Loading picker state...");
                if (UseIsolatedStorage)
                {
                    using (Stream stream = await Windows.Storage.ApplicationData.Current.LocalFolder.SafeOpenStreamForReadAsync("./Resources/DefaultPickerState_1.7.xml"))
                    {
                        Type[] extraTypes = new[] { typeof(OldFavoriteSet), typeof(OldFavoriteSetting), typeof(PickerOption), typeof(ListPickerOption), typeof(BooleanPickerOption), };
                        XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                        overrides.Add(typeof(OldFavoriteSet), new XmlAttributes
                        {
                            XmlRoot = new XmlRootAttribute("FavoriteSet")
                        });
                        overrides.Add(typeof(OldFavoriteSetting), new XmlAttributes
                        {
                            XmlRoot = new XmlRootAttribute("FavoriteSetting")
                        });
                        XmlSerializer serializer = new XmlSerializer(typeof(PickerState), overrides, extraTypes, null, null);
                        state = serializer.Deserialize(stream) as PickerState;
                    }
                }
            }
            catch (IsolatedStorageException)
            {
                AppLog.Instance.Error("IsolatedStorageException while loading Picker State.");
                // Just ignore the exception
            }
            return state;
        }

        public async void Save()
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
                            await GenericContractSerializer.Serialize(stream, this);
                        }
                    }
                }
            }
            catch (IsolatedStorageException e)
            {
                AppLog.Instance.Error("Unable to save picker state", e);
            }
        }

        public static async void SaveDefault()
        {
            string data;
            using (Stream stream = await Windows.Storage.ApplicationData.Current.LocalFolder.SafeOpenStreamForReadAsync("./Resources/OldPickerState.xml"))
            {
                StreamReader reader = new StreamReader(stream);
                data = reader.ReadToEnd();
            }
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = store.OpenFile(PickerStateFileName, FileMode.Create))
                {
                    StreamWriter w = new StreamWriter(file);
                    w.Write(data);
                    w.Flush();
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
            catch (IsolatedStorageException)
            {
            }
        }
    }
}