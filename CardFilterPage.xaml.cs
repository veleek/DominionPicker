using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Ben.Dominion
{
    public partial class CardFilterPage : PhoneApplicationPage
    {
        private static readonly List<CardType> FilterTypes = new List<CardType>
        {
            CardType.Action,
            CardType.Attack,
            CardType.Duration,
            CardType.Reaction,
            CardType.Treasure,
            CardType.Victory,
        };


        public CardFilterPage()
        {
            InitializeComponent();

            SetFilter.ItemsSource = Cards.AllSets;
            SetFilter.SummaryForSelectedItemsDelegate = list =>
            {
                if (list == null || list.Count == 0)
                {
                    return "All Sets";
                }
                return list.Cast<CardSet>().Select(s => s.ToString()).Aggregate((a,b) => a + ", " + b);
            };

            TypeFilter.ItemsSource = FilterTypes;
            TypeFilter.SummaryForSelectedItemsDelegate = list =>
            {
                if (list == null || list.Count == 0)
                {
                    return "All Types";
                }
                return list.Cast<CardType>().Select(s => s.ToString()).Aggregate((a, b) => a + ", " + b);
            };
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCardsList();
        }

        private void UpdateCardsList()
        {
            IEnumerable<CardSet> selectedSets = null;
            CardType selectedTypes = CardType.None;

            if (SetFilter.SelectedItems == null || SetFilter.SelectedItems.Count == 0)
            {
                selectedSets = Cards.AllSets;
            }
            else
            {
                selectedSets = SetFilter.SelectedItems.Cast<CardSet>().ToList();
            }

            if (TypeFilter.SelectedItems == null || TypeFilter.SelectedItems.Count == 0)
            {
                selectedTypes = FilterTypes.Aggregate((a,b) => a | b);
            }
            else
            {
                selectedTypes = TypeFilter.SelectedItems.Cast<CardType>().Aggregate((a, b) => a | b);
            }

            var cards = Cards.AllCards
                .Where(c => c.InSet(selectedSets) && c.IsType(selectedTypes))
                //.OrderBy(c => c.Name)
                .GroupBy(c => c.Set, (set, cs) => new CardGrouping(set, cs));

            CardsList.ItemsSource = cards;
        }

        public class CardGrouping : IGrouping<CardSet, Card>
        {
            public CardGrouping(CardSet key, IEnumerable<Card> cards)
            {
                this.Key = key;
                this.Cards = cards;
            }

            public CardSet Key { get; private set; }
            public IEnumerable<Card> Cards { get; private set; }

            public override bool Equals(object obj)
            {
                CardGrouping otherGroup = obj as CardGrouping;

                return (otherGroup != null) && (otherGroup.Key == this.Key);
            }

            public override int GetHashCode()
            {
                return this.Key.GetHashCode();
            }

            public IEnumerator<Card> GetEnumerator()
            {
                return Cards.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}