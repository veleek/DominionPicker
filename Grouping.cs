using System.Collections.Generic;
using System.Linq;

namespace Ben.Data
{
    /// <summary>
    /// Implementation of the generic grouping interface that can be used for binding
    /// grouped elements to a LongListSelector in a meaningful way.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            this.Key = key;
            this.Elements = elements;
        }

        public TKey Key { get; private set; }
        public IEnumerable<TElement> Elements { get; private set; }

        public override bool Equals(object obj)
        {
            var otherGroup = obj as Grouping<TKey, TElement>;

            return (otherGroup != null) && (otherGroup.Key.Equals(this.Key));
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
