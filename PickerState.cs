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

        public static PickerState Load()
        {
            return Load(PickerStateFileName);
        }

        public static PickerState Load(string fileName)
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
                            if (store.FileExists(fileName))
                            {
                                using (Stream stream = store.OpenFile(fileName, FileMode.Open))
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

            return state;
        }

        public static PickerState LoadDefault()
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
                        using (Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream("./Assets/DefaultPickerState_1.7.xml"))
                        {
                            state = GenericContractSerializer.Deserialize<PickerState>(stream);
                        }
                    }
                }
            }
            catch (IsolatedStorageException)
            {
                Debug.WriteLine("IsolatedStorageException while loading Picker State.");
                // Just ignore the exception
            }

            return state;
        }

        #endregion Statics

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

        public PickerState()
        {
            //this.Favorites = new ObservableCollection<PickerFavorite>();
            this.FavoriteSettings = new ObservableCollection<OldFavoriteSetting>();
            this.FavoriteSets = new ObservableCollection<OldFavoriteSet>();
        }
    }
}
