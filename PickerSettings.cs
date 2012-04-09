using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ben.Dominion
{
    public class PickerSettings
    {
        public static PickerSettings DefaultSettings
        {
            get
            {
                PickerSettings settings = new PickerSettings();

                settings.Sets = Cards.AllSets.Select(s => new SetSelector(s)).ToList();
                // Easy way to deselect the 'promo' card set
                settings.Sets.Single(s => s.Set == CardSet.Promo).IsSelected = false;

                settings.MinimumCardsPerSet = new ListPickerOption<Int32>("Minimum cards per set", new List<Int32> { 2, 3, 4, 5, 10 }, 3);
                settings.RequireDefense = new BooleanPickerOption("If there's an attack,\nrequire a defense card", false);
                settings.RequireTrash = new BooleanPickerOption("Require a card that lets you\ntrash other cards", false);
                settings.PlusBuys = new PolicyOption("+Buys Policy");
                settings.PlusBuys.Notes = "This may take a bit longer";
                settings.PlusActions = new PolicyOption("+Actions Policy");
                settings.PlusActions.Notes = "This may take a bit longer";
                settings.PlusCoins = new PolicyOption("+Coins Policy");

                settings.FilterPotions = new BooleanPickerOption("Filter Cards that Cost Potions");
                settings.FilterAction = new BooleanPickerOption("Filter Action Cards");
                settings.FilterAttack = new BooleanPickerOption("Filter Attack Cards");
                settings.FilterDuration = new BooleanPickerOption("Filter Duration Cards");
                settings.FilterReaction = new BooleanPickerOption("Filter Reaction Cards");
                settings.FilterTreasure = new BooleanPickerOption("Filter Treasure Cards");
                settings.FilterVictory = new BooleanPickerOption("Filter Victory Cards");

                return settings;
            }
        }

        public List<SetSelector> Sets { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]
        public List<CardSet> SelectedSets
        {
            get
            {
                return Sets.Where(s => s.IsSelected).Select(s => s.Set).ToList();
            }
        }

        public CardType SelectedTypes
        {
            get
            {
                CardType selectedTypes = CardType.None;

                if (!FilterAction.IsEnabled) { selectedTypes |= CardType.Action; }
                if (!FilterAttack.IsEnabled) { selectedTypes |= CardType.Attack; }
                if (!FilterDuration.IsEnabled) { selectedTypes |= CardType.Duration; }
                if (!FilterReaction.IsEnabled) { selectedTypes |= CardType.Reaction; }
                if (!FilterTreasure.IsEnabled) { selectedTypes |= CardType.Treasure; }
                if (!FilterVictory.IsEnabled) { selectedTypes |= CardType.Victory; }

                return selectedTypes;
            }
        }

        public CardType FilteredTypes
        {
            get
            {
                CardType filteredTypes = CardType.None;

                if (FilterAction.IsEnabled) { filteredTypes |= CardType.Action; }
                if (FilterAttack.IsEnabled) { filteredTypes |= CardType.Attack; }
                if (FilterDuration.IsEnabled) { filteredTypes |= CardType.Duration; }
                if (FilterReaction.IsEnabled) { filteredTypes |= CardType.Reaction; }
                if (FilterTreasure.IsEnabled) { filteredTypes |= CardType.Treasure; }
                if (FilterVictory.IsEnabled) { filteredTypes |= CardType.Victory; }

                return filteredTypes;
            }
        }

        public ListPickerOption<Int32> MinimumCardsPerSet { get; set; }
        public BooleanPickerOption RequireDefense { get; set; }
        public BooleanPickerOption RequireTrash { get; set; }
        public PolicyOption PlusBuys { get; set; }
        public PolicyOption PlusActions { get; set; }
        public PolicyOption PlusCoins { get; set; }

        public BooleanPickerOption FilterPotions { get; set; }
        public BooleanPickerOption FilterAction { get; set; }
        public BooleanPickerOption FilterAttack { get; set; }
        public BooleanPickerOption FilterDuration { get; set; }
        public BooleanPickerOption FilterReaction { get; set; }
        public BooleanPickerOption FilterTreasure { get; set; }
        public BooleanPickerOption FilterVictory { get; set; }

        private List<PickerOption> allOptions = null;
        [XmlIgnore]
        [IgnoreDataMember]
        public List<PickerOption> AllOptions
        {
            get
            {
                if (allOptions == null)
                {
                    allOptions = new List<PickerOption> 
                    { 
                        MinimumCardsPerSet,
                        RequireDefense,
                        RequireTrash,
                        PlusBuys,
                        PlusActions,
                        //PlusCoins,
                        FilterPotions,
                        FilterAction,
                        FilterAttack,
                        FilterDuration,
                        FilterReaction,
                        FilterTreasure,
                        FilterVictory,
                    };
                }

                return allOptions;
            }
        }

        public PickerSettings() { }

        public PickerSettings Clone()
        {
            PickerSettings clone = new PickerSettings();
            clone.Sets = this.Sets.Select(s => new SetSelector(s.Set, s.IsSelected)).ToList();
            clone.MinimumCardsPerSet = this.MinimumCardsPerSet.Clone<ListPickerOption<Int32>>();
            clone.RequireDefense = this.RequireDefense.Clone<BooleanPickerOption>();
            clone.RequireTrash = this.RequireTrash.Clone<BooleanPickerOption>();
            clone.PlusBuys = this.PlusBuys.Clone<PolicyOption>();
            clone.PlusActions = this.PlusActions.Clone<PolicyOption>();
            clone.PlusCoins = this.PlusCoins.Clone<PolicyOption>();

            clone.FilterPotions = this.FilterPotions.Clone<BooleanPickerOption>();
            clone.FilterAction = this.FilterAction.Clone<BooleanPickerOption>();
            clone.FilterAttack = this.FilterAttack.Clone<BooleanPickerOption>();
            clone.FilterDuration = this.FilterDuration.Clone<BooleanPickerOption>();
            clone.FilterReaction = this.FilterReaction.Clone<BooleanPickerOption>();
            clone.FilterTreasure = this.FilterTreasure.Clone<BooleanPickerOption>();
            clone.FilterVictory = this.FilterVictory.Clone<BooleanPickerOption>();

            return clone;
        }

        public override string ToString()
        {
            return SelectedSets.Select(s => s.ToString().Substring(0,4)).Aggregate((a, b) => a + ", " + b);
        }
    }

    public class SetSelector
    {
        [XmlAttribute]
        public CardSet Set { get; set; }
        [XmlAttribute]
        public Boolean IsSelected { get; set; }

        public SetSelector() { }
        public SetSelector(CardSet set) : this(set, true) { }
        public SetSelector(CardSet set, Boolean selected)
        {
            this.Set = set;
            this.IsSelected = selected;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Set, IsSelected);
        }
    }
}
