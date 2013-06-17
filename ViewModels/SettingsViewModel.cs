using Ben.Dominion.Models;
using Ben.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Ben.Dominion.ViewModels
{
    public class SettingsViewModel : NotifyPropertyChangedBase
    {
        private ListOption<int> minimumCardsPerSet;

        private bool requireDefense;
        private bool requireTrash = true;

        private PlusOption plusBuys;
        private PlusOption plusActions;

        private bool showExtras;
        private bool pickPlatinumColony;
        private bool pickShelterOrEstate;

        private List<SetOption> sets = Cards.AllSets.Select(s => new SetOption { Set = s }).ToList();

        [XmlIgnore]
        public List<SetOption> Sets
        {
            get { return sets; }
            set { SetProperty(ref sets, value, "Sets"); }
        }

        public List<CardSet> SelectedSets
        {
            get { return Sets.Select(s => s.Set).ToList(); }
            set
            {
                foreach (var set in sets)
                {
                    set.Enabled = value.Contains(set.Set);
                }
            }
        }


        [XmlIgnore]
        public List<PlusOptionValue> PlusOptionValues
        {
            get
            {
                return new List<PlusOptionValue>
                {
                    PlusOptionValue.Require,
                    PlusOptionValue.RequirePlus2,
                    PlusOptionValue.Prevent,
                    PlusOptionValue.PreventPlus2,
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

        public class SetOption : NotifyPropertyChangedBase
        {
            private CardSet set;
            private string setName;
            private bool enabled;

            public bool Enabled
            {
                get { return enabled; }
                set { SetProperty(ref enabled, value, "Enabled"); }
            }

            public CardSet Set
            {
                get { return set; }
                set { SetProperty(ref set, value, "Set"); }
            }

            public string SetName
            {
                get { return setName; }
                set { SetProperty(ref setName, value, "SetName"); }
            }
        }

        public class ListOption<TOptionType> : NotifyPropertyChangedBase
        {
            private bool enabled;
            private TOptionType optionValue;

            public bool Enabled
            {
                get { return enabled; }
                set { SetProperty(ref enabled, value, "Enabled"); }
            }

            [XmlText]
            public TOptionType OptionValue
            {
                get { return optionValue; }
                set { SetProperty(ref optionValue, value, "OptionValue"); }
            }
        }

        public class PlusOption : ListOption<PlusOptionValue>
        {
        }

        public enum PlusOptionValue
        {
            Require,
            RequirePlus2,
            Prevent,
            PreventPlus2
        }
    }
}
