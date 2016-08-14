using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Ben.Dominion.Models;

namespace Ben.Dominion
{

   public class PickerSettings
   {
      private List<SetSelector> sets;

      public PickerSettings()
      {
      }

      public static PickerSettings DefaultSettings
      {
         get
         {
            PickerSettings settings = new PickerSettings();
            settings.Sets = ConfigurationModel.Instance.OwnedSets.Select(s => new SetSelector(s)).ToList();
            // Easy way to deselect the 'promo' card set
            settings.Sets.Single(s => s.Set == CardSet.Promo).IsSelected = false;
            settings.MinimumCardsPerSet = new ListPickerOption<Int32>("Minimum cards per set", new List<Int32>
               {
                  2,
                  3,
                  4,
                  5,
                  10
               }
               , 3);
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
            settings.PickPlatinumColony = new BooleanPickerOption("Pick if Platinum and Colony are used", true);
            settings.PickSheltersOrEstates = new BooleanPickerOption("Pick if Shelters or Estates are used", true);
            return settings;
         }
      }

      public List<SetSelector> Sets
      {
         get
         {
            return this.sets;
         }
         set
         {
            var ownedSets = ConfigurationModel.Instance.OwnedSets;
            this.sets = value.Where(s => ownedSets.Contains(s.Set)).ToList();
         }
      }

      [XmlIgnore]
      [IgnoreDataMember]
      public List<CardSet> SelectedSets
      {
         get
         {
            return this.Sets.Where(s => s.IsSelected).Select(s => s.Set).ToList();
         }
      }

      private List<Card> filteredCards;

      [XmlIgnore]
      public List<Card> FilteredCards
      {
         get
         {
            if ( this.filteredCards == null )
            {
               this.filteredCards = this.FilteredCardIds.Select(cid => Cards.Lookup[cid]).ToList();
            }
            return this.filteredCards;
         }
         set
         {
            this.FilteredCardIds = value.Select(c => c.ID).ToList();
         }
      }

      private List<Int32> filteredCardIds;

      public List<Int32> FilteredCardIds
      {
         get
         {
            return (this.filteredCardIds = this.filteredCardIds ?? new List<Int32>());
         }
         set
         {
            this.filteredCardIds = value;
            this.filteredCards = null;
         }
      }

      public CardType SelectedTypes
      {
         get
         {
            CardType selectedTypes = CardType.None;
            if ( !this.FilterAction.IsEnabled )
            {
               selectedTypes |= CardType.Action;
            }
            if ( !this.FilterAttack.IsEnabled )
            {
               selectedTypes |= CardType.Attack;
            }
            if ( !this.FilterDuration.IsEnabled )
            {
               selectedTypes |= CardType.Duration;
            }
            if ( !this.FilterReaction.IsEnabled )
            {
               selectedTypes |= CardType.Reaction;
            }
            if ( !this.FilterTreasure.IsEnabled )
            {
               selectedTypes |= CardType.Treasure;
            }
            if ( !this.FilterVictory.IsEnabled )
            {
               selectedTypes |= CardType.Victory;
            }
            return selectedTypes;
         }
      }

      public CardType FilteredTypes
      {
         get
         {
            CardType filteredTypes = CardType.None;
            if ( this.FilterAction.IsEnabled )
            {
               filteredTypes |= CardType.Action;
            }
            if ( this.FilterAttack.IsEnabled )
            {
               filteredTypes |= CardType.Attack;
            }
            if ( this.FilterDuration.IsEnabled )
            {
               filteredTypes |= CardType.Duration;
            }
            if ( this.FilterReaction.IsEnabled )
            {
               filteredTypes |= CardType.Reaction;
            }
            if ( this.FilterTreasure.IsEnabled )
            {
               filteredTypes |= CardType.Treasure;
            }
            if ( this.FilterVictory.IsEnabled )
            {
               filteredTypes |= CardType.Victory;
            }
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

      public BooleanPickerOption PickPlatinumColony { get; set; }

      public BooleanPickerOption PickSheltersOrEstates { get; set; }

      private List<PickerOption> allOptions = null;

      [XmlIgnore]
      [IgnoreDataMember]
      public List<PickerOption> AllOptions
      {
         get
         {
            if ( this.allOptions == null )
            {
               this.allOptions = new List<PickerOption>
                  {
                     this.MinimumCardsPerSet,
                     this.RequireDefense,
                     this.RequireTrash,
                     this.PlusBuys,
                     this.PlusActions,
                     this.PickPlatinumColony,
                     this.PickSheltersOrEstates,
                  //PlusCoins,
                  //FilterPotions,
                  //FilterAction,
                  //FilterAttack,
                  //FilterDuration,
                  //FilterReaction,
                  //FilterTreasure,
                  //FilterVictory,
                  }
                  ;
            }
            return this.allOptions;
         }
      }

      public PickerSettings Clone()
      {
         PickerSettings clone = new PickerSettings();
         clone.Sets = Cards.AllSets.Select(s => new SetSelector(s, this.SelectedSets.Contains(s))).ToList();
         //clone.Sets = this.Sets.Select(s => new SetSelector(s.Set, s.IsSelected)).ToList();
         clone.MinimumCardsPerSet = this.MinimumCardsPerSet.Clone();
         clone.RequireDefense = this.RequireDefense.Clone();
         clone.RequireTrash = this.RequireTrash.Clone();
         clone.PlusBuys = this.PlusBuys.Clone();
         clone.PlusActions = this.PlusActions.Clone();
         clone.PlusCoins = this.PlusCoins.Clone();
         clone.FilterPotions = this.FilterPotions.Clone();
         clone.FilterAction = this.FilterAction.Clone();
         clone.FilterAttack = this.FilterAttack.Clone();
         clone.FilterDuration = this.FilterDuration.Clone();
         clone.FilterReaction = this.FilterReaction.Clone();
         clone.FilterTreasure = this.FilterTreasure.Clone();
         clone.FilterVictory = this.FilterVictory.Clone();
         clone.PickPlatinumColony = this.PickPlatinumColony.Clone();
         clone.PickSheltersOrEstates = this.PickSheltersOrEstates.Clone();
         clone.FilteredCardIds = new List<int>(this.FilteredCardIds);
         return clone;
      }

      public override string ToString()
      {
         return this.SelectedSets.Select(s => s.ToString().Substring(0, 4)).Aggregate((a, b) => a + ", " + b);
      }

   }

   public class SetSelector
   {

      [XmlAttribute]
      public CardSet Set { get; set; }

      [XmlAttribute]
      public Boolean IsSelected { get; set; }


      public SetSelector()
      {
      }

      public SetSelector(CardSet set)
      : this(set, true)
      {
      }

      public SetSelector(CardSet set, Boolean selected)
      {
         this.Set = set;
         this.IsSelected = selected;
      }

      public override string ToString()
      {
         return String.Format("{0} - {1}", this.Set, this.IsSelected);
      }

   }

}