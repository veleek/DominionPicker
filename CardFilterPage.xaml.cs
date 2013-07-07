using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private const string FilteredCardsSeachFilter = "<filtered>";

        private CardSelector[] cardSelectors;
        private List<CardGrouping> cardSelectorGroups;
        private List<CardGrouping> filteredCardSelectorGroups;
        private Dictionary<CardSet, CardGrouping> filteredGroupsMap;
        private BackgroundWorker filterWorker;
        private string currentFilter;
        private Queue<CardSelector> changedCards = new Queue<CardSelector>();

        public CardFilterPage()
        {
            InitializeComponent();

            filterWorker = new BackgroundWorker();
            filterWorker.WorkerSupportsCancellation = true;
            filterWorker.DoWork += new DoWorkEventHandler(filterWorker_DoWork);

            // Generate 'card selectors' for each card.  A card selector is just an object
            // that pairs a boolean value that indicates whether a card is filtered or not
            // with a specific card and it can be used to databind with.
            cardSelectors = Cards.AllCards // Cards.PickableCards
                .OrderBy(c => c.Name).OrderBy(c => c.Set)
                .Select(c => new CardSelector(c, false))
                .ToArray();

            cardSelectorGroups = cardSelectors.GroupBy(c => c.Card.Set, (set, setCards) => new CardGrouping(set, setCards)).ToList();
            filteredCardSelectorGroups = cardSelectors.GroupBy(c => c.Card.Set, (set, setCards) => new CardGrouping(set, setCards)).ToList();
            filteredGroupsMap = filteredCardSelectorGroups.ToDictionary(g => g.Key);

            CardsList.ItemsSource = filteredCardSelectorGroups;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Set all the selected values by loading the filtered cards
            LoadFilteredCards();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Saves the list of filtered cards into the settings
            MainViewModel.Instance.Settings.FilteredCards = new CardList(cardSelectors.Where(fc => fc.Selected).Select(c => c.Card));
        }

        /// <summary>
        /// Reset the list of filtered cards
        /// </summary>
        private void ResetFilteredCards()
        {
            MainViewModel.Instance.Settings.FilteredCards = new CardList();

            LoadFilteredCards();

            ClearFilter();
        }

        /// <summary>
        /// Load the list of filtered cards from the settings object and sets the values 
        /// on the card selectors
        /// </summary>
        private void LoadFilteredCards()
        {
            var filteredCards = MainViewModel.Instance.Settings.FilteredCards;

            foreach (var cardSelector in cardSelectors)
            {
                cardSelector.Selected = filteredCards.Contains(cardSelector.Card);
            }
        }

        /// <summary>
        /// BackgroundWorker function to perform filtering on all of the cards based
        /// on the current search filter.
        /// </summary>
        /// <param name="sender">The background worker that requested the filtering</param>
        /// <param name="e">BackgroundWorker event args (unused)</param>
        /// <remarks>
        /// Actually filtering the cards is relatively expensive, so all that work is done
        /// on the background worker and the filtered state is saved on the selectors, then
        /// the dispatcher is invoked to actually filter the cards from the long list 
        /// selector (add/remove from groups) since that needs to happen on the UI thread.
        /// </remarks>
        void filterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (currentFilter == String.Empty)
            {
                foreach (var card in cardSelectors)
                {
                    if (card.Filter(false))
                    {
                        changedCards.Enqueue(card);
                    }
                }
            }
            else if (currentFilter == FilteredCardsSeachFilter)
            {
                foreach (var card in cardSelectors)
                {
                    if (card.Filter(!card.Selected))
                    {
                        changedCards.Enqueue(card);
                    }
                }    
            }
            else
            {
                foreach (var card in cardSelectors)
                {
                    // Cancel the filtering if we need to
                    if (filterWorker.CancellationPending)
                    {
                        return;
                    }

                    if (card.Filter(!card.Card.ContainsText(currentFilter)))
                    {
                        changedCards.Enqueue(card);
                    }
                }
            }

            if (!filterWorker.CancellationPending)
            {
                UpdateCardsList();
            }
        }

        private void FilterCardsList(String newFilter)
        {
            if (newFilter == currentFilter && newFilter != String.Empty)
            {
                return;
            }

            if (filterWorker.IsBusy)
            {
                filterWorker.CancelAsync();

                while (filterWorker.IsBusy)
                {
                }
            }

            currentFilter = newFilter;

            filterWorker.RunWorkerAsync();
        }

        private void ClearFilter()
        {
            if (SearchTextBox.Text == String.Empty)
            {
                // Manually filter on empty
                FilterCardsList(String.Empty);
            }
            else
            {
                SearchTextBox.Text = String.Empty;
            }
        }

        /// <summary>
        /// Invokes a call on the UI thread to go through the cards and add/remove
        /// filtered cards from the LongListSelector as appropriate.
        /// </summary>
        /// <remarks>
        /// It would be nice to only loop through cards that were changed, but there's
        /// not an easy way to manage the actual indexes of the cards without manually
        /// implementing the observable collection and 
        /// </remarks>
        private void UpdateCardsList()
        {
            //UpdateCardsListMin();
            //return;

            Dispatcher.BeginInvoke(() =>
            {
                for (int i = 0; i < cardSelectorGroups.Count; i++)
                {
                    var baseGroup = cardSelectorGroups[i];
                    var filteredGroup = filteredCardSelectorGroups[i];

                    int index = 0;
                    // We have to loop through every card in the group
                    // so that we have an index for the next visible card
                    // otherwise we don't know where in the list to add the 
                    // item 
                    foreach (var c in baseGroup)
                    {
                        if (filterWorker.CancellationPending)
                        {
                            return;
                        }

                        bool shouldBeVisible = !c.Filtered;
                        bool isVisible = filteredGroup.Contains(c);

                        if (shouldBeVisible)
                        {
                            if (!isVisible)
                            {
                                filteredGroup.Insert(index, c);
                            }
                            index++;
                        }
                        else
                        {
                            if (isVisible)
                            {
                                filteredGroup.Remove(c);
                            }
                        }
                    }
                }
            });
        }

        private void UpdateCardsListMin()
        {
            Dispatcher.BeginInvoke(() =>
            {
                while (changedCards.Count > 0)
                {
                    var card = changedCards.Dequeue();
                    var group = filteredGroupsMap[card.Card.Set];

                    if (card.Filtered)
                    {
                        group.Remove(card);
                    }
                    else
                    {
                        group.SortedInsert(card);
                    }
                }
            });
        }
        
        private void UpdateCardsList2()
        {
            String filterText = SearchTextBox.Text;

            for (int i = 0; i < cardSelectorGroups.Count; i++)
            {
                var baseGroup = cardSelectorGroups[i];
                var filteredGroup = filteredCardSelectorGroups[i];

                int index = 0;
                foreach (var c in baseGroup)
                {
                    bool shouldBeVisible = c.Card.Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
                    /*
                    c.Card.InSet(selectedSets) && c.Card.IsType(selectedTypes) 
                    && c.Card.Name.IndexOf(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0
                    && c.Card.Rules.IndexOf(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                     */

                    bool isVisible = filteredGroup.Contains(c);

                    if (shouldBeVisible)
                    {
                        if (!isVisible)
                        {
                            filteredGroup.Insert(index, c);
                        }
                        index++;
                    }
                    else
                    {
                        if (isVisible)
                        {
                            filteredGroup.Remove(c);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// An event handler to listen for changes to the search box text and dynamically filter the cards list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCardsList(SearchTextBox.Text);
        }

        /// <summary>
        /// An event handler to listen for keys on the text box to handle closing the filter if you press enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CardsList.Focus();
            }
        }

        /// <summary>
        /// Called when the SearchBox action icon is tapped to clear the current search filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            ClearFilter();
        }

        /// <summary>
        /// Called to show only the currently filtered cards in the list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFilteredCards_Click(object sender, EventArgs e)
        {
            FilterCardsList(FilteredCardsSeachFilter);
            //SearchTextBox.Text = FilteredCardsSeachFilter;
        }

        private void ResetFilteredCards_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("This will clear all cards you have previously chosen to filter.  Do you want to continue?", "Warning!", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                ResetFilteredCards();
            }
        }

        private void CardItemDetails_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).SelectedCard = sender.GetContext<CardSelector>().Card;
            NavigationService.Navigate("/CardInfo.xaml");
        }

        private void About_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate("/AboutPage.xaml");
        }

        private static GroupedCollectionViewSource<CardSet> cardsViewSource;
        public static GroupedCollectionViewSource<CardSet> CardsViewSource
        {
            get
            {
                if (cardsViewSource == null)
                {
                    cardsViewSource = new GroupedCollectionViewSource<CardSet>
                    {
                        GroupDescriptions =
                        {
                            new PropertyGroupDescription("Card.Set"),
                        },
                        SortDescriptions =
                        {
                            new SortDescription("Card.Set", ListSortDirection.Ascending),
                            new SortDescription("Card.Name", ListSortDirection.Ascending),
                        },
                        Source = Cards.AllCards.Select(c => new CardSelector(c, false)).ToList()
                    };
                }

                return cardsViewSource;
            }
        }
    }
}