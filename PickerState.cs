using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Ben.Dominion
{
    public class PickerState : INotifyPropertyChanged
    {
        private PickerSettings currentSettings;
        public PickerSettings CurrentSettings
        {
            get { return currentSettings; }
            private set
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

        /// <summary>
        /// The current resultant card list
        /// </summary>
        public ObservableCollection<Card> CardList { get; set; }

        public void SaveFavorite()
        {
            FavoriteSettings.Add(CurrentSettings.Clone());
        }

        public void LoadSettings(Int32 favoriteIndex)
        {
            LoadSettings(FavoriteSettings[favoriteIndex]);
        }

        public void LoadSettings(PickerSettings settings)
        {
            CurrentSettings = settings;
        }

        public void CreateCardList()
        {
            CreateCardList(CurrentSettings);
        }

        public void CreateCardList(PickerSettings settings)
        {
            List<Card> cardPool = Cards.AllCards.Where(c => c.InSet(settings.Sets)).ToList();
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
    }
}
