using Ben.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ben.Dominion
{
    public class SettingsViewModel : NotifyPropertyChangedBase
    {
        private ListOption<int> minimumCardsPerSet = new ListOption<int> { Enabled = true, OptionValue = 5 };
        private List<SetOption> sets = Cards.AllSets.Select(s => new SetOption { Set = s, Enabled = s != CardSet.Promo }).ToList();
        private CardList filteredCards = new CardList();

        private bool requireDefense = false;
        private bool requireTrash = false;

        private PlusOption plusBuys = new PlusOption() { Enabled = false, OptionValue = "Require" };
        private PlusOption plusActions = new PlusOption() { Enabled = false, OptionValue = "Require" };

        private bool pickPlatinumColony = true;
        private bool pickShelterOrEstate = true;
        private bool showExtras = true;

        public SettingsViewModel()
        {
        }

        [XmlIgnore]
        public List<SetOption> Sets
        {
            get { return sets; }
            set { SetProperty(ref sets, value, "Sets"); }
        }

        [XmlIgnore]
        public List<CardSet> SelectedSets
        {
            get { return Sets.Where(s => s.Enabled).Select(s => s.Set).ToList(); }
            set
            {
                foreach (var set in sets)
                {
                    set.Enabled = value.Contains(set.Set);
                }

                NotifyPropertyChanged("Sets");
            }
        }

        [XmlElement("SelectedSets")]
        public string SelectedSetNames
        {
            get 
            {
                return string.Join(",", SelectedSets.Select(s => s.ToString()));
            }

            set
            {
                foreach (var set in sets)
                {
                    set.Enabled = value.Contains(set.Set.ToString());
                }

                NotifyPropertyChanged("Sets");
            }
        }

        public CardList FilteredCards
        {
            get
            {
                return filteredCards;
            }

            set
            {
                this.SetProperty(ref filteredCards, value, "FilteredCards");
            }
        }
        
        [XmlIgnore]
        public List<string> PlusOptionValues
        {
            get
            {
                return new List<string>
                {
                    "Require",
                    "Require +2",
                    "Prevent",
                    "Prevent +2",
                };
            }
        }

        [XmlIgnore]
        public int[] MinimumCardsPerSetValues
        {
            get
            {
                return new[] { 1, 2, 3, 4, 5, 10 }; 
            }
        }

        public ListOption<int> MinimumCardsPerSet
        {
            get { return minimumCardsPerSet; }
            set { SetProperty(ref minimumCardsPerSet, value, "MinimumCardsPerSet"); }
        }

        public bool RequireDefense
        {
            get { return requireDefense; }
            set { SetProperty(ref requireDefense, value, "RequireDefense"); }
        }

        public bool RequireTrash
        {
            get { return requireTrash; }
            set { SetProperty(ref requireTrash, value, "RequireTrash"); }
        }

        public PlusOption PlusBuys
        {
            get { return plusBuys; }
            set { SetProperty(ref plusBuys, value, "PlusBuys"); }
        }

        public PlusOption PlusActions
        {
            get { return plusActions; }
            set { SetProperty(ref plusActions, value, "PlusActions"); }
        }

        public bool ShowExtras
        {
            get { return showExtras; }
            set { SetProperty(ref showExtras, value, "ShowExtras"); }
        }

        public bool PickPlatinumColony
        {
            get { return pickPlatinumColony; }
            set { SetProperty(ref pickPlatinumColony, value, "PickPlatinumColony"); }
        }

        public bool PickShelterOrEstate
        {
            get { return pickShelterOrEstate; }
            set { SetProperty(ref pickShelterOrEstate, value, "PickShelterOrEstate"); }
        }

        public override string ToString()
        {
            return this.SelectedSets.Select(s => s.ToString().Substring(0, 4)).Aggregate((a, b) => a + ", " + b);
            //return this.SelectedSetNames;
        }
    }
}
