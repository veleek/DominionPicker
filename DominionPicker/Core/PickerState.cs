using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Serialization;
using Ben.Utilities;
using Microsoft.Xna.Framework;

namespace Ben.Dominion
{
    [XmlRoot(Namespace = "")]
    public class PickerState : NotifyPropertyChangedBase
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
            DateTime start = DateTime.UtcNow;
            try
            {
                if (UseIsolatedStorage)
                {
                    if (state == null)
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

        public static PickerState LoadDefault()
        {
            PickerState state = null;
            DateTime start = DateTime.UtcNow;
            try
            {
                AppLog.Instance.Log("Loading picker state...");
                if (UseIsolatedStorage)
                {
                    if (state == null)
                    {
                        using (Stream stream = TitleContainer.OpenStream("./Resources/DefaultPickerState_1.7.xml"))
                        {
                            Type[] extraTypes = new[]
                            {
                                typeof (OldFavoriteSet),
                                typeof (OldFavoriteSetting),
                                typeof (PickerOption),
                                typeof (ListPickerOption),
                                typeof (BooleanPickerOption),
                            };

                            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                            overrides.Add(typeof (OldFavoriteSet), new XmlAttributes { XmlRoot = new XmlRootAttribute("FavoriteSet") });
                            overrides.Add(typeof (OldFavoriteSetting), new XmlAttributes { XmlRoot = new XmlRootAttribute("FavoriteSetting") });

                            XmlSerializer serializer = new XmlSerializer(typeof (PickerState), overrides, extraTypes, null, null);

                            state = serializer.Deserialize(stream) as PickerState;
                        }
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
                            GenericContractSerializer.Serialize(stream, this);
                        }
                    }
                }
            }
            catch (IsolatedStorageException e)
            {
                AppLog.Instance.Error("Unable to save picker state", e);
            }
        }

        public static void SaveDefault()
        {
            string data;
            using (Stream stream = TitleContainer.OpenStream("./Resources/OldPickerState.xml"))
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

        public static void MergeWithNewState()
        {
            var oldState = Load();
            if (oldState == null)
            {
                return;
            }
            
            // There is an old save file, so let's merge the settings
            foreach (var oldSetting in oldState.FavoriteSettings)
            {
                try
                {
                    if (MainViewModel.Instance.Favorites.FavoriteSettings.All(f => f.Name != oldSetting.Name))
                    {
                        PickerSettings oldSettings = oldSetting.Value;
                        SettingsViewModel newSettings = new SettingsViewModel
                        {
                            SelectedSets = oldSettings.SelectedSets,
                            MinimumCardsPerSet = new ListOption<int> { Enabled = oldSettings.MinimumCardsPerSet.IsEnabled, OptionValue = oldSettings.MinimumCardsPerSet.SelectedValue },
                            RequireDefense = oldSettings.RequireDefense.IsEnabled,
                            RequireTrash = oldSettings.RequireTrash.IsEnabled,
                            PlusActions = new PlusOption { Enabled = oldSettings.PlusActions.IsEnabled, OptionValue = oldSettings.PlusActions.SelectedValue },
                            PlusBuys = new PlusOption { Enabled = oldSettings.PlusBuys.IsEnabled, OptionValue = oldSettings.PlusBuys.SelectedValue },
                            PickPlatinumColony = oldSettings.PickPlatinumColony.IsEnabled,
                            PickShelterOrEstate = oldSettings.PickSheltersOrEstates.IsEnabled,
                            ShowExtras = true,
                        };

                        FavoriteSetting newSetting = new FavoriteSetting(oldSetting.Name, newSettings);
                        MainViewModel.Instance.Favorites.FavoriteSettings.Add(newSetting);
                    }
                }
                catch (Exception e)
                {
                    AppLog.Instance.Error(string.Format("There was an issue loading favorite setting {0}.\r\n{1}", oldSetting.Name, e));
                }
            }

            foreach (var oldSet in oldState.FavoriteSets)
            {
                try
                {
                    FavoriteSet newSet = new FavoriteSet(oldSet.Name, oldSet.Result);

                    if (!MainViewModel.Instance.Favorites.FavoriteSets.Any(s => newSet.Value.Cards.All(c => s.Value.Cards.Names.Contains(c.Name))))
                    {
                        MainViewModel.Instance.Favorites.FavoriteSets.Add(newSet);
                    }
                }
                catch (Exception e)
                {
                    AppLog.Instance.Error(string.Format("There was an issue loading favorite set {0}.\r\n{1}", oldSet.Name, e));
                }
            }

            ClearSavedState();
        }
    }
}