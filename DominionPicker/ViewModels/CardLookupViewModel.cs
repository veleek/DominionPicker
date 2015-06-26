using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Ben.Data;
using GalaSoft.MvvmLight.Threading;

namespace Ben.Dominion.ViewModels
{
    public class CardLookupViewModel
    {
        public const string FilteredCardsSeachFilter = "<filtered>";

        private static CardLookupViewModel instance;
        private static GroupedCollectionViewSource<CardSet> cardsViewSource;

        private readonly BackgroundWorker filterWorker;
        private readonly Queue<CardSelector> changedCards = new Queue<CardSelector>();
        private readonly List<CardSetGrouping> cardSelectorGroups;
        private readonly Dictionary<CardSet, CardSetGrouping> filteredGroupsMap;

        private string currentFilter;

        public CardLookupViewModel()
        {
            this.filterWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            this.filterWorker.DoWork += this.filterWorker_DoWork;

            // Generate 'card selectors' for each card.  A card selector is just an object
            // that pairs a boolean value that indicates whether a card is filtered or not
            // with a specific card and it can be used to databind with.
            this.CardSelectors = Cards.AllCards // Cards.PickableCards
                .OrderBy(c => c.Name).ThenBy(c => c.Set)
                .Select(c => new CardSelector(c, false))
                .ToArray();

            this.cardSelectorGroups = this.CardSelectors
                .GroupBy(c => c.Card.Set, (set, setCards) => new CardSetGrouping(set, setCards))
                .OrderBy(g => g.Key)
                .ToList();
            // Note, this is is a duplicate of the previous because we want to construct an 
            // explicitly separate set of CardGroupings so that we can modify one while using
            // the others as part of our filtering process.
            this.FilteredCardSelectorGroups = this.CardSelectors
                .GroupBy(c => c.Card.Set, (set, setCards) => new CardSetGrouping(set, setCards))
                .OrderBy(g => g.Key)
                .ToList();
            this.filteredGroupsMap = this.FilteredCardSelectorGroups.ToDictionary(g => g.Key);

        }

        public static CardLookupViewModel Instance => instance ?? (instance = new CardLookupViewModel());

        public CardSelector[] CardSelectors { get; }

        public List<CardSetGrouping> FilteredCardSelectorGroups { get; }

        public CardList FilteredCards
        {
            get
            {
                return new CardList(this.CardSelectors.Where(fc => fc.Selected).Select(c => c.Card));
            }
        }

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

        /// <summary>
        /// Reset the list of filtered cards
        /// </summary>
        public void ResetFilteredCards()
        {
            MainViewModel.Instance.Settings.FilteredCards = new CardList();

            this.LoadFilteredCards();
        }

        /// <summary>
        /// Load the list of filtered cards from the settings object and sets the values 
        /// on the card selectors
        /// </summary>
        public void LoadFilteredCards()
        {
            var filteredCards = MainViewModel.Instance.Settings.FilteredCards;

            foreach (var cardSelector in this.CardSelectors)
            {
                cardSelector.Selected = filteredCards.Contains(cardSelector.Card);
            }
        }

        /// <summary>
        /// Saves the list of filtered cards into the settings
        /// </summary>
        public void SaveFilteredCards()
        {
            MainViewModel.Instance.Settings.FilteredCards = this.FilteredCards;
        }


        public void FilterCardsList(String newFilter)
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
                foreach (var card in this.CardSelectors)
                {
                    if (card.Filter(false))
                    {
                        this.changedCards.Enqueue(card);
                    }
                }
            }
            else if (this.currentFilter == FilteredCardsSeachFilter)
            {
                foreach (var card in this.CardSelectors)
                {
                    if (card.Filter(!card.Selected))
                    {
                        this.changedCards.Enqueue(card);
                    }
                }
            }
            else
            {
                foreach (var card in this.CardSelectors)
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

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                for (int i = 0; i < this.cardSelectorGroups.Count; i++)
                {
                    var baseGroup = this.cardSelectorGroups[i];
                    var filteredGroup = this.FilteredCardSelectorGroups[i];

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
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
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
            String filterText = this.currentFilter;

            for (int i = 0; i < this.cardSelectorGroups.Count; i++)
            {
                var baseGroup = this.cardSelectorGroups[i];
                var filteredGroup = this.FilteredCardSelectorGroups[i];

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
    }
}
