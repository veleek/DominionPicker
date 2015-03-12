using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ben.Data;
using Ben.Dominion.Resources;
using Ben.Dominion.Views;
using Ben.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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

        private readonly CardSelector[] cardSelectors;
        private readonly List<CardGrouping> cardSelectorGroups;
        private readonly List<CardGrouping> filteredCardSelectorGroups;
        private readonly Dictionary<CardSet, CardGrouping> filteredGroupsMap;
        private readonly BackgroundWorker filterWorker;
        private readonly Queue<CardSelector> changedCards = new Queue<CardSelector>();
        private string currentFilter;

        public CardFilterPage()
        {
            this.InitializeComponent();

            this.filterWorker = new BackgroundWorker();
            this.filterWorker.WorkerSupportsCancellation = true;
            this.filterWorker.DoWork += new DoWorkEventHandler(this.filterWorker_DoWork);

            // Generate 'card selectors' for each card.  A card selector is just an object
            // that pairs a boolean value that indicates whether a card is filtered or not
            // with a specific card and it can be used to databind with.
            this.cardSelectors = Cards.AllCards // Cards.PickableCards
                .OrderBy(c => c.Name).ThenBy(c => c.Set)
                .Select(c => new CardSelector(c, false))
                .ToArray();

            this.cardSelectorGroups =
                this.cardSelectors.GroupBy(c => c.Card.Set, (set, setCards) => new CardGrouping(set, setCards)).ToList();
            this.filteredCardSelectorGroups =
                this.cardSelectors.GroupBy(c => c.Card.Set, (set, setCards) => new CardGrouping(set, setCards)).ToList();
            this.filteredGroupsMap = this.filteredCardSelectorGroups.ToDictionary(g => g.Key);

            this.CardsList.ItemsSource = this.filteredCardSelectorGroups;

            var resetFilteredMenuItem = new ApplicationBarMenuItem {Text = Strings.Lookup_ResetFiltered};
            resetFilteredMenuItem.Click += this.ResetFilteredCards_Click;
            this.ApplicationBar.MenuItems.Add(resetFilteredMenuItem);

            var showFilteredMenuItem = new ApplicationBarMenuItem {Text = Strings.Lookup_ShowFiltered};
            showFilteredMenuItem.Click += this.ShowFilteredCards_Click;
            this.ApplicationBar.MenuItems.Add(showFilteredMenuItem);

            var aboutMenuItem = new ApplicationBarMenuItem {Text = Strings.Menu_About};
            aboutMenuItem.Click += this.About_Click;
            this.ApplicationBar.MenuItems.Add(aboutMenuItem);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Set all the selected values by loading the filtered cards
            this.LoadFilteredCards();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Saves the list of filtered cards into the settings
            MainViewModel.Instance.Settings.FilteredCards =
                new CardList(this.cardSelectors.Where(fc => fc.Selected).Select(c => c.Card));
        }

        /// <summary>
        /// Reset the list of filtered cards
        /// </summary>
        private void ResetFilteredCards()
        {
            MainViewModel.Instance.Settings.FilteredCards = new CardList();

            this.LoadFilteredCards();

            this.ClearFilter();
        }

        /// <summary>
        /// Load the list of filtered cards from the settings object and sets the values 
        /// on the card selectors
        /// </summary>
        private void LoadFilteredCards()
        {
            var filteredCards = MainViewModel.Instance.Settings.FilteredCards;

            foreach (var cardSelector in this.cardSelectors)
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
        private void filterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.currentFilter == String.Empty)
            {
                foreach (var card in this.cardSelectors)
                {
                    if (card.Filter(false))
                    {
                        this.changedCards.Enqueue(card);
                    }
                }
            }
            else if (this.currentFilter == FilteredCardsSeachFilter)
            {
                foreach (var card in this.cardSelectors)
                {
                    if (card.Filter(!card.Selected))
                    {
                        this.changedCards.Enqueue(card);
                    }
                }
            }
            else
            {
                foreach (var card in this.cardSelectors)
                {
                    // Cancel the filtering if we need to
                    if (this.filterWorker.CancellationPending)
                    {
                        return;
                    }

                    if (card.Filter(!card.Card.ContainsText(this.currentFilter)))
                    {
                        this.changedCards.Enqueue(card);
                    }
                }
            }

            if (!this.filterWorker.CancellationPending)
            {
                this.UpdateCardsList();
            }
        }

        private void FilterCardsList(String newFilter)
        {
            if (newFilter == this.currentFilter && newFilter != String.Empty)
            {
                return;
            }

            if (this.filterWorker.IsBusy)
            {
                this.filterWorker.CancelAsync();

                while (this.filterWorker.IsBusy)
                {
                }
            }

            this.currentFilter = newFilter;

            this.filterWorker.RunWorkerAsync();
        }

        private void ClearFilter()
        {
            if (this.SearchTextBox.Text == String.Empty)
            {
                // Manually filter on empty
                this.FilterCardsList(String.Empty);
            }
            else
            {
                this.SearchTextBox.Text = String.Empty;
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

            this.Dispatcher.BeginInvoke(() =>
            {
                for (int i = 0; i < this.cardSelectorGroups.Count; i++)
                {
                    var baseGroup = this.cardSelectorGroups[i];
                    var filteredGroup = this.filteredCardSelectorGroups[i];

                    int index = 0;
                    // We have to loop through every card in the group
                    // so that we have an index for the next visible card
                    // otherwise we don't know where in the list to add the 
                    // item 
                    foreach (var c in baseGroup)
                    {
                        if (this.filterWorker.CancellationPending)
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
            this.Dispatcher.BeginInvoke(() =>
            {
                while (this.changedCards.Count > 0)
                {
                    var card = this.changedCards.Dequeue();
                    var group = this.filteredGroupsMap[card.Card.Set];

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
            String filterText = this.SearchTextBox.Text;

            for (int i = 0; i < this.cardSelectorGroups.Count; i++)
            {
                var baseGroup = this.cardSelectorGroups[i];
                var filteredGroup = this.filteredCardSelectorGroups[i];

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
            this.FilterCardsList(this.SearchTextBox.Text);
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
                this.CardsList.Focus();
            }
        }

        /// <summary>
        /// Called when the SearchBox action icon is tapped to clear the current search filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            this.ClearFilter();
        }

        /// <summary>
        /// Called to show only the currently filtered cards in the list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFilteredCards_Click(object sender, EventArgs e)
        {
            this.FilterCardsList(FilteredCardsSeachFilter);
            //SearchTextBox.Text = FilteredCardsSeachFilter;
        }

        private void ResetFilteredCards_Click(object sender, EventArgs e)
        {
            var result =
                MessageBox.Show(
                    "This will clear all cards you have previously chosen to filter.  Do you want to continue?",
                    "Warning!", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.ResetFilteredCards();
            }
        }

        private void CardItemDetails_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).SelectedCard = sender.GetContext<CardSelector>().Card;
            PickerView.CardInfo.Go();
        }

        private void About_Click(object sender, EventArgs e)
        {
            PickerView.About.Go();
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