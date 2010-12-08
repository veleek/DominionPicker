using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ben.Dominion
{
    public class PickerSettings
    {
        public String Name { get; set; }
        public List<SetSelector> Sets { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]
        public List<CardSet> SelectedSets
        {
            get
            {
                return Sets.Where(s => s.IsSelected).Select(s => s.Set).ToList();
            }
            /*
            set
            {
                Sets = Cards.AllSets.Select(s => new SetSelector(s, value.Contains(s))).ToList();
            }
             */
        }

        public String SetsString
        {
            get
            {
                return SelectedSets.Select(s => s.ToString().Substring(0,4)).Aggregate((a, b) => a + ", " + b);
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

        public static List<String> PolicyOptions = new List<String> { "Require", "Prevent" };

        public ListPickerOption<Int32> MinimumCardsPerSet { get; set; }
        public BooleanPickerOption RequireDefense { get; set; }
        public PolicyOption PlusBuys { get; set; }
        public PolicyOption PlusActions { get; set; }
        public PolicyOption PlusCoins { get; set; }

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
                        PlusBuys,
                        PlusActions,
                        PlusCoins,
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

        public PickerSettings()
        {
            Name = "Default Settings";
            Sets = Cards.AllSets.Select(s => new SetSelector(s)).ToList();

            MinimumCardsPerSet = new ListPickerOption<Int32>("Minimum cards per set", new List<Int32> { 2, 3, 5, 10 }, 3);
            RequireDefense = new BooleanPickerOption("If there's an attack,\nrequire a defense card", false);
            PlusBuys = new PolicyOption("+Buys Policy");
            PlusBuys.Notes = "This may take a long time";
            PlusActions = new PolicyOption("+Actions Policy");
            PlusActions.Notes = "This may take a long time";
            PlusCoins = new PolicyOption("+Coins Policy");

            FilterAction = new BooleanPickerOption("Filter Action Cards");
            FilterAttack = new BooleanPickerOption("Filter Attack Cards");
            FilterDuration = new BooleanPickerOption("Filter Duration Cards");
            FilterReaction = new BooleanPickerOption("Filter Reaction Cards");
            FilterTreasure = new BooleanPickerOption("Filter Treasure Cards");
            FilterVictory = new BooleanPickerOption("Filter Victory Cards");

        }

        public PickerSettings Clone()
        {
            PickerSettings clone = new PickerSettings();
            clone.Sets = this.Sets.Select(s => new SetSelector(s.Set, s.IsSelected)).ToList();
            clone.MinimumCardsPerSet = this.MinimumCardsPerSet.Clone() as ListPickerOption<Int32>;
            clone.RequireDefense = this.RequireDefense.Clone() as BooleanPickerOption;
            clone.PlusBuys = this.PlusBuys.Clone<PolicyOption>();
            clone.PlusActions = this.PlusActions.Clone<PolicyOption>();
            clone.PlusCoins = this.PlusCoins.Clone<PolicyOption>();

            clone.FilterAction = this.FilterAction.Clone<BooleanPickerOption>();
            clone.FilterAttack = this.FilterAttack.Clone<BooleanPickerOption>();
            clone.FilterDuration = this.FilterDuration.Clone<BooleanPickerOption>();
            clone.FilterReaction = this.FilterReaction.Clone<BooleanPickerOption>();
            clone.FilterTreasure = this.FilterTreasure.Clone<BooleanPickerOption>();
            clone.FilterVictory = this.FilterVictory.Clone<BooleanPickerOption>();

            return clone;
        }
    }

    public class SetSelector
    {
        public CardSet Set { get; set; }
        public Boolean IsSelected { get; set; }

        public SetSelector() { }
        public SetSelector(CardSet set) : this(set, true) { }
        public SetSelector(CardSet set, Boolean selected)
        {
            this.Set = set;
            this.IsSelected = selected;
        }
    }
}
