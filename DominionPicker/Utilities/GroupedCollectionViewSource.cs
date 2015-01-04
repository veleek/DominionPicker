using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Ben.Utilities;

namespace Ben.Data
{
    public class GroupedCollectionViewSource<TKey> : CollectionViewSource
    {
        public ICollection<IGrouping<TKey, object>> GroupedView
        {
            get
            {
                return this.View.Groups.Select(g => 
                {
                    var group = g as CollectionViewGroup;
                    return (IGrouping<TKey, object>)new Grouping<TKey, object>((TKey)group.Name, group.Items);
                }).ToObservableCollection();
            }
        }
    }
}
