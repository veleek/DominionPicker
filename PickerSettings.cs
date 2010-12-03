using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ben.Dominion
{
    [XmlInclude(typeof(BooleanPickerOption))]
    [XmlInclude(typeof(IntPickerOption))]
    [KnownType(typeof(BooleanPickerOption))]
    [KnownType(typeof(IntPickerOption))]
    public class PickerSettings
    {
        public String Name { get; set; }

        [XmlIgnore]
        [IgnoreDataMember]
        public List<SetSelector> Sets { get; set; }

        public List<CardSet> SelectedSets
        {
            get
            {
                return Sets.Where(s => s.IsSelected).Select(s => s.Set).ToList();
            }
            set
            {
                Sets = Cards.AllSets.Select(s => new SetSelector(s, value.Contains(s))).ToList();
            }
        }

        public IntPickerOption MinimumCardsPerSet { get; set; }
        public BooleanPickerOption RequirePlusActions { get; set; }
        public BooleanPickerOption RequirePlusBuys { get; set; } 
        public BooleanPickerOption RequireDefense { get; set; }
                
        [XmlIgnore]
        public List<PickerOption> AllOptions
        {
            get
            {
                return new List<PickerOption> 
                { 
                    MinimumCardsPerSet,
                    RequirePlusActions,
                    RequirePlusBuys,
                    RequireDefense,
                };
            }
        }

        public PickerSettings()
        {
            Sets = Cards.AllSets.Select(s => new SetSelector(s)).ToList();

            MinimumCardsPerSet = new IntPickerOption("Minimum cards per set", 3, Enumerable.Range(1, 5).ToList());
            RequirePlusActions = new BooleanPickerOption("Require a card that gives actions", false);
            RequirePlusBuys = new BooleanPickerOption("Require a card that gives buys", false);
            RequireDefense = new BooleanPickerOption("If there's an attack,\nrequire a defense card", false);
        }

        public PickerSettings Clone()
        {
            PickerSettings clone = new PickerSettings();
            clone.Sets = this.Sets.Select(s => new SetSelector(s.Set, s.IsSelected)).ToList();
            clone.MinimumCardsPerSet = this.MinimumCardsPerSet.Clone() as IntPickerOption;
            clone.RequirePlusActions = this.RequirePlusActions.Clone() as BooleanPickerOption;
            clone.RequirePlusBuys = this.RequirePlusBuys.Clone() as BooleanPickerOption;
            clone.RequireDefense = this.RequireDefense.Clone() as BooleanPickerOption;

            return clone;
        }
    }

    public class SetSelector
    {
        public CardSet Set { get; set; }
        public Boolean IsSelected { get; set; }

        public SetSelector(CardSet set) : this(set, true) { }
        public SetSelector(CardSet set, Boolean selected)
        {
            this.Set = set;
            this.IsSelected = selected;
        }
    }
}
