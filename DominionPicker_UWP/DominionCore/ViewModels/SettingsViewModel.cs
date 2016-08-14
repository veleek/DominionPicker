using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Ben.Dominion.Models;
using Ben.Utilities;

namespace Ben.Dominion
{
    public class SettingsViewModel
       : NotifyPropertyChangedBase
    {
        private ListOption<int> minimumCardsPerSet = new ListOption<int>
        {
            Enabled = true,
            OptionValue = 5
        };
        private List<SetOption> sets;
        private CardList filteredCards = new CardList();
        private bool requireDefense;
        private bool requireTrash;
        private CardRequirementOption plusBuysOption;
        private CardRequirementOption plusActionsOption;

        private bool pickPlatinumColony = true;
        private bool pickShelterOrEstate = true;
        private bool showExtras = true;

        public SettingsViewModel()
        {
            ConfigurationModel.Instance.PropertyChanged += (sender, e) =>
               {
                   if (e.PropertyName == "OwnedSets")
                   {
                       this.sets = null;
                   }
               };
        }

        [XmlIgnore]
        public List<SetOption> Sets
        {
            get
            {
                if (this.sets == null)
                {
                    this.sets = ConfigurationModel.Instance.OwnedSets.Select(s => new SetOption
                    {
                        Set = s,
                        Enabled = s != CardSet.Promo
                    }).ToList();
                }
                return this.sets;
            }
            set
            {
                this.SetProperty(ref this.sets, value);
            }
        }

        [XmlIgnore]
        public List<CardSet> SelectedSets
        {
            get
            {
                return this.Sets.Where(s => s.Enabled).Select(s => s.Set).ToList();
            }
            set
            {
                foreach (var set in this.Sets)
                {
                    set.Enabled = value.Contains(set.Set);
                }
                this.OnPropertyChanged("Sets");
            }
        }

        [XmlElement("SelectedSets")]
        public string SelectedSetNames
        {
            get
            {
                return string.Join(",", this.SelectedSets.Select(s => s.ToString()));
            }
            set
            {
                foreach (var set in this.Sets)
                {
                    set.Enabled = value.Contains(set.Set.ToString());
                }
                this.OnPropertyChanged("Sets");
            }
        }

        [XmlIgnore]
        public List<CardSet> PinnedSets
        {
            get
            {
                return this.Sets.Where(s => s.Required).Select(s => s.Set).ToList();
            }
            set
            {
                foreach (var set in this.Sets)
                {
                    set.Required = value.Contains(set.Set);
                }
                this.OnPropertyChanged("Sets");
            }
        }

        [XmlElement("PinnedSets")]
        public string PinnedSetNames
        {
            get
            {
                return string.Join(",", this.PinnedSets.Select(s => s.ToString()));
            }
            set
            {
                foreach (var set in this.Sets)
                {
                    set.Required = value.Contains(set.Set.ToString());
                }
                this.OnPropertyChanged("Sets");
            }
        }

        public CardList FilteredCards
        {
            get
            {
                return this.filteredCards;
            }
            set
            {
                this.SetProperty(ref this.filteredCards, value);
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
            get
            {
                return this.minimumCardsPerSet;
            }
            set
            {
                this.SetProperty(ref this.minimumCardsPerSet, value);
            }
        }

        public bool RequireDefense
        {
            get
            {
                return this.requireDefense;
            }
            set
            {
                this.SetProperty(ref this.requireDefense, value);
            }
        }

        public bool RequireTrash
        {
            get
            {
                return this.requireTrash;
            }
            set
            {
                this.SetProperty(ref this.requireTrash, value);
            }
        }

        public CardRequirementOption PlusBuysOption
        {
            get
            {
                return this.plusBuysOption;
            }
            set
            {
                this.SetProperty(ref this.plusBuysOption, value);
            }
        }

        public CardRequirementOption PlusActionsOption
        {
            get
            {
                return this.plusActionsOption;
            }
            set
            {
                this.SetProperty(ref this.plusActionsOption, value);
            }
        }

        public override string ToString()
        {
            return this.SelectedSets.Select(s => s.ToString().Substring(0, 4)).Aggregate((a, b) => a + ", " + b);
        }
    }
}