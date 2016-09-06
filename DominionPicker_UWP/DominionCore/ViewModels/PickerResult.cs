using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Ben.Utilities;
using Ben.Dominion.ViewModels;
using System.Collections.Specialized;

namespace Ben.Dominion.ViewModels
{

   public class PickerResult
      : NotifyPropertyChangedBase
   {
      private static readonly Regex actionRegex = new Regex(@"\+[1-9] Action");
      private static readonly Regex twoActionRegex = new Regex(@"\+[2-9] Actions");
      private static readonly Regex buyRegex = new Regex(@"\+[1-9] Buy");
      private static readonly Regex twoBuyRegex = new Regex(@"\+[2-9] Buys");
      private static readonly Regex trashRegex = new Regex(@"Trash(?! this)");
      private CardList pool = null;
      private CardList cards = null;
      private List<CardGrouping<CardGroup>> groupedCards = null;
      private List<string> additionalStuff = null;
      private ResultSortOrder sortOrder;

      public PickerResult()
      {
      }

      public CardList Pool
      {
         get
         {
            return pool;
         }
         set
         {
            this.SetProperty(ref pool, value);
         }
      }

      public CardList Cards
      {
         get
         {
            return cards;
         }
         set
         {
            var oldCards = cards;
            if ( this.SetProperty(ref cards, value) )
            {
               if ( oldCards != null )
               {
                  oldCards.CollectionChanged -= Cards_CollectionChanged;
               }
               if ( cards != null )
               {
                  cards.CollectionChanged += Cards_CollectionChanged;
               }
               this.GroupedCards = null;
            }
         }
      }

      private void Cards_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if ( e.Action == NotifyCollectionChangedAction.Replace )
         {
            // We want to special case replacements to maintain 'ordering' in the card groups
            Card originalCard = e.OldItems[0] as Card;
            Card replacementCard = e.NewItems[0] as Card;
            CardGrouping<CardGroup> replacementGroup = this.GroupedCards.First(g => g.Key == replacementCard.Group);
            int index = replacementGroup.IndexOf(originalCard);
            replacementGroup[index] = replacementCard;
         }
         else
         {
            // Just null out the whole list and let it get regenerated.
            this.GroupedCards = null;
         }
      }

      [XmlIgnore]
      public List<CardGrouping<CardGroup>> GroupedCards
      {
         get
         {
            if ( this.groupedCards == null )
            {
               this.groupedCards = this.cards.OrderBy(c => c.DisplayName).GroupBy(c => c.Group, (g, cards) => new CardGrouping<CardGroup>(g, new CardList(cards))).OrderBy(g => g.Key.Type).ToList();
            }
            return this.groupedCards;
         }
         set
         {
            this.SetProperty(ref this.groupedCards, value);
         }
      }

      public List<String> AdditionalStuff
      {
         get
         {
            return additionalStuff;
         }
         set
         {
            this.SetProperty(ref additionalStuff, value);
         }
      }

      public ResultSortOrder SortOrder
      {
         get
         {
            return this.sortOrder;
         }
         set
         {
            this.SetProperty(ref this.sortOrder, value);
         }
      }

      public bool HasCardType(CardType type)
      {
         return this.HasCard(c => c.IsType(type));
      }

      public bool HasCard(Card card)
      {
         return this.Cards.Contains(card, CardIdComparer.Default);
      }

      public bool HasCard(String name)
      {
         return this.HasCard(Card.FromName(name));
      }

      public bool HasCard(Func<Card, bool> predicate)
      {
         return this.Cards.Any(predicate);
      }

      public IEnumerable<Card> CardsOfType(CardType type)
      {
         return this.CardsWhere(c => c.IsType(type));
      }

      public IEnumerable<Card> CardsWhere(Func<Card, bool> predicate)
      {
         return this.Cards.Where(predicate);
      }

      public Boolean HasAttack
      {
         get
         {
            return HasCardType(CardType.Attack);
         }
      }

      public Boolean HasReaction
      {
         get
         {
            return HasCardType(CardType.Reaction);
         }
      }

      public Boolean HasReactionOrLighthouse
      {
         get
         {
            return this.HasReaction || this.HasCard("Lighthouse");
         }
      }

      public Boolean HasTrash
      {
         get
         {
            return Cards.Any(c => trashRegex.IsMatch(c.Rules));
         }
      }

      public Boolean HasPlusAction
      {
         get
         {
            return Cards.Any(c => actionRegex.IsMatch(c.Rules));
         }
      }

      public Boolean HasPlus2Action
      {
         get
         {
            return Cards.Any(c => twoActionRegex.IsMatch(c.Rules));
         }
      }

      public Boolean HasPlusBuy
      {
         get
         {
            return Cards.Any(c => buyRegex.IsMatch(c.Rules));
         }
      }

      public Boolean HasPlus2Buy
      {
         get
         {
            return Cards.Any(c => twoBuyRegex.IsMatch(c.Rules));
         }
      }

      public Boolean HasExtras
      {
         get
         {
            return this.AdditionalStuff != null && this.AdditionalStuff.Count > 0;
         }
      }

      public void Replace(Card target)
      {
         if ( target == null )
         {
            return ;
         }
         var replacement = Pool.GetRandomCardExcept(Cards);
         if ( replacement == null )
         {
            return ;
         }
         Replace(target, replacement);
      }

      public void Replace(Card target, Card replacement)
      {
         // You can only replace kingdom cards.
         if ( target.Group.Type != CardGroupType.KingdomCard )
         {
            return ;
         }
         int index = Cards.IndexOf(target);
         if ( index == -1 )
            System.Diagnostics.Debugger.Break();
         CardGroup group = target.Group;
         target.Group = null;
         replacement.Group = group;
         Pool.Remove(replacement);
         Pool.Add(target);
         Cards[index] = replacement;
      }

      public void Sort(ResultSortOrder newSortOrder)
      {
         if ( Cards == null )
         {
            return ;
         }
         this.sortOrder = newSortOrder != ResultSortOrder.None ? newSortOrder : ResultSortOrder.Name;
         switch ( this.sortOrder )
         {
            case ResultSortOrder.Cost:
               Cards = Cards.OrderBy(c => c.Cost).ToCardList();
               break;
            case ResultSortOrder.Set:
               // Order them by set then alphabetically
               // This makes the most sense because it's probably 
               // being used by someone who has the cards in the 
               // original set boxes
               Cards = Cards.OrderBy(c => c.Set + "," + c.DisplayName).ToCardList();
               break;
            case ResultSortOrder.Name:
            default:
               Cards = Cards.OrderBy(c => c.DisplayName).ToCardList();
               // Orders them randomly, not used
               ////Cards = Cards.OrderBy(c => Guid.NewGuid()).ToCardList();
               break;
         }
      }

      public override string ToString()
      {
         if ( this.Cards == null )
         {
            return "Cards is Null";
         }
         return this.Cards.Select(c => c.Set).Distinct().Select(s => s.ToString().Substring(0, 4)).OrderBy(s => s).Aggregate((a, b) => a + ", " + b);
      }

      public List<int> ToList()
      {
         return this.Cards == null ? new List<int>() : this.Cards.Select(c => c.ID).ToList();
      }

      public static PickerResult FromList(IEnumerable<Int32> cardIds)
      {
         PickerResult result = new PickerResult
            {
               Cards = cardIds.Select(Card.FromId).ToCardList()
            };
         return result;
      }

   }

}