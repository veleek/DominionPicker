using System;
using System.Collections.Generic;
using System.Linq;

namespace Ben.Dominion
{
    public static class EnumerableCardExtensions
    {
        public static Card GetRandomCard(this IEnumerable<Card> pool)
        {
            return pool.OrderBy(c => Guid.NewGuid()).FirstOrDefault();
        }

        public static Card GetRandomCard(this IEnumerable<Card> pool, Func<Card, bool> predicate)
        {
            return pool.Where(predicate).GetRandomCard();
        }

        public static Card GetRandomCard(this IEnumerable<Card> pool, CardType type)
        {
            return pool.Where(c => c.IsType(type)).GetRandomCard();
        }

        public static Card GetRandomCard(this IEnumerable<Card> pool, CardSet set)
        {
            return pool.Where(c => c.InSet(set)).GetRandomCard();
        }

        public static Card GetRandomCardExcept(this IEnumerable<Card> pool, params Card[] others)
        {
            return pool.GetRandomCardExcept((IEnumerable<Card>)others);
        }

        public static Card GetRandomCardExcept(this IEnumerable<Card> pool, IEnumerable<Card> others)
        {
            // Get all the cards in the pool other than the ones in the list randomly ordered
            var cards = pool.Except(others, CardIdComparer.Default);

            return cards.GetRandomCard();
        }

        public static void Move<T>(this IEnumerable<T> items, IList<T> source, IList<T> destination)
        {
            foreach (T item in items.ToArray())
            {
                source.Remove(item);
                destination.Add(item);
            }
        }

        public static void Move(this Card item, IList<Card> source, IList<Card> destination)
        {
            source.Remove(item);
            destination.Add(item);
        }
    }
}
