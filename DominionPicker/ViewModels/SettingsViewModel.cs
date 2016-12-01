﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Ben.Dominion.Models;
using Ben.Utilities;

namespace Ben.Dominion
{
	public class SettingsViewModel : NotifyPropertyChangedBase
	{
		private ListOption<int> minimumCardsPerSet = new ListOption<int> {Enabled = true, OptionValue = 5};

        private List<SetOption> sets;

		private CardList filteredCards = new CardList();

		private bool requireDefense;
		private bool requireTrash;

		private PlusOption plusBuys = new PlusOption() {Enabled = false, OptionValue = "Require"};
		private PlusOption plusActions = new PlusOption() {Enabled = false, OptionValue = "Require"};

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
                    this.sets = Cards.AllSets.Select(s => new SetOption { Set = s, Enabled = s != CardSet.Promo }).ToList();
                }

                return this.sets;
            }
            set { this.SetProperty(ref this.sets, value); }
        }

		[XmlIgnore]
		public List<CardSet> SelectedSets
		{
			get { return this.Sets.Where(s => s.Enabled).Select(s => s.Set).ToList(); }
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
			get { return string.Join(",", this.SelectedSets.Select(s => s.ToString())); }

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
            get { return this.Sets.Where(s => s.Required).Select(s => s.Set).ToList(); }
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
            get { return string.Join(",", this.PinnedSets.Select(s => s.ToString())); }

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
			get { return this.filteredCards; }

			set { this.SetProperty(ref this.filteredCards, value); }
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
			get { return new[] {1, 2, 3, 4, 5, 10}; }
		}

		public ListOption<int> MinimumCardsPerSet
		{
			get { return this.minimumCardsPerSet; }
			set { this.SetProperty(ref this.minimumCardsPerSet, value); }
		}

		public bool RequireDefense
		{
			get { return this.requireDefense; }
			set { this.SetProperty(ref this.requireDefense, value); }
		}

		public bool RequireTrash
		{
			get { return this.requireTrash; }
			set { this.SetProperty(ref this.requireTrash, value); }
		}

		public PlusOption PlusBuys
		{
			get { return this.plusBuys; }
			set { this.SetProperty(ref this.plusBuys, value); }
		}

		public PlusOption PlusActions
		{
			get { return this.plusActions; }
			set { this.SetProperty(ref this.plusActions, value); }
		}

		public bool ShowExtras
		{
			get { return this.showExtras; }
			set { this.SetProperty(ref this.showExtras, value); }
		}

		public bool PickPlatinumColony
		{
			get { return this.pickPlatinumColony; }
			set { this.SetProperty(ref this.pickPlatinumColony, value); }
		}

		public bool PickShelterOrEstate
		{
			get { return this.pickShelterOrEstate; }
			set { this.SetProperty(ref this.pickShelterOrEstate, value); }
		}

		public override string ToString()
		{
			return this.SelectedSets.Select(s => s.ToString().Substring(0, 4)).Aggregate((a, b) => a + ", " + b);
		}
	}
}