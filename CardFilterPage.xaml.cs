using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ben.Data;
using Ben.Utilities;
using Microsoft.Phone.Controls;

namespace Ben.Dominion
{
    public partial class CardFilterPage : PhoneApplicationPage
    {
        private static readonly List<CardType> FilterTypes = new List<CardType>
        {
            CardType.Treasure,
            CardType.Victory,
            CardType.Action,
            CardType.Attack,
            CardType.Reaction,
            CardType.Duration,
            CardType.Looter,
        };

        private CardSelector[] cardSelectors;

        public CardFilterPage()
        {
            InitializeComponent();

            // Generate 'card selectors' for each card.  A card selector is just an object
            // that pairs a boolean value that indicates whether a card is filtered or not
            // with a specific card and it can be used to databind with.
            cardSelectors = Cards.PickableCards
                .OrderBy(c => c.Name).OrderBy(c => c.Set)
                .Select(c => new CardSelector(c, false))
                .ToArray();

            // Initialize the filter stuff
            SetFilter.ItemsSource = Cards.AllSets;
            SetFilter.SummaryForSelectedItemsDelegate = list =>
            {
                if (list == null || list.Count == 0)
                {
                    return "All Sets";
                }
                return list.Cast<CardSet>().Select(s => s.ToString()).Aggregate((a, b) => a + ", " + b);
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Set all the selected values by loading the filtered cards
            LoadFilteredCards();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Saves the list of filtered cards into the settings
            PickerState.Current.CurrentSettings.FilteredCards = cardSelectors.Where(fc => fc.Selected).Select(c => c.Card).ToList();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCardsList();
            RootPivot.SelectedIndex = 0;
        }

        /// <summary>
        /// Resets the list filters so that all cards are being shown in the card list.  
        /// This has nothing to do with actually excluding cards from the selection process.
        /// </summary>
        private void ResetListFilter()
        {
            // A little heavy handed, but it works
            SetFilter.SetValue(ListPicker.SelectedItemsProperty, new ObservableCollection<object>());
            TypeFilter.SetValue(ListPicker.SelectedItemsProperty, new ObservableCollection<object>());
            
            // The following method does not work because for some reason 
            // the selection changed handler doesn't get called when you 
            // modify the collection manually
            //SetFilter.SelectedItems.Clear();
        }

        /// <summary>
        /// Reset the list of filtered cards
        /// </summary>
        private void ResetFilteredCards()
        {
            PickerState.Current.CurrentSettings.FilteredCardIds = null;

            LoadFilteredCards();
        }

        /// <summary>
        /// Load the list of filtered cards from the settings object and sets the values 
        /// on the card selectors
        /// </summary>
        private void LoadFilteredCards()
        {
            var filteredCards = PickerState.Current.CurrentSettings.FilteredCards;

            foreach (var cardSelector in cardSelectors)
            {
                cardSelector.Selected = filteredCards.Contains(cardSelector.Card);
            }

            UpdateCardsList();
        }

        /// <summary>
        /// Filters the list of all the selectors down to the ones specified by the list filters
        /// and groups them appropriately and then add them to the long list selector
        /// </summary>
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

            var cards = cardSelectors
                .Where(c => c.Card.InSet(selectedSets) && c.Card.IsType(selectedTypes))
                .GroupBy(c => c.Card.Set, (set, setCards) => new CardGrouping(set, setCards));

            CardsList.ItemsSource = cards;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource vc = new CollectionViewSource()
            {
                
            };
        }

        private void ResetFilteredCards_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("This will clear all cards you have previously chosen to filter.  Do you want to continue?", "Warning!", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                ResetFilteredCards();
            }
        }

        private void ResetListFilter_Click(object sender, EventArgs e)
        {
            ResetListFilter();
        }

        private void About_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate("/AboutPage.xaml");
        }

        /// <summary>
        /// A grouping of card selectors by set
        /// </summary>
        public class CardGrouping : Grouping<CardSet, CardSelector>
        {
            public CardGrouping(CardSet key, IEnumerable<CardSelector> cards)
                : base(key, cards)
            {
            }
        }
    }

    /// <summary>
    /// A simple key value pair to joins a specific card with a filtered status 
    /// that can be used for data binding a check box.
    /// </summary>
    public class CardSelector
    {
        public CardSelector() { }
        public CardSelector(Card card, bool selected)
        {
            this.Card = card;
            this.Selected = selected;
        }

        public Card Card { get; set; }
        public bool Selected { get; set; }
    }
}