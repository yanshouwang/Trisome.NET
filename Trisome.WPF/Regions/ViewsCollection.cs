using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Trisome.WPF.Regions
{
    /// <summary>
    /// Implementation of <see cref="IViewsCollection"/> that takes an <see cref="ObservableCollection{T}"/> of <see cref="ItemMetadata"/>
    /// and filters it to display an <see cref="INotifyCollectionChanged"/> collection of
    /// <see cref="object"/> elements (the items which the <see cref="ItemMetadata"/> wraps).
    /// </summary>
    public class ViewsCollection : IViewsCollection
    {
        readonly ObservableCollection<ItemMetadata> _subjectCollection;

        readonly Dictionary<ItemMetadata, MonitorInfo> _monitoredItems =
            new Dictionary<ItemMetadata, MonitorInfo>();

        readonly Predicate<ItemMetadata> _filter;
        Comparison<object> _sort;
        List<object> _filteredItems = new List<object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewsCollection"/> class.
        /// </summary>
        /// <param name="list">The list to wrap and filter.</param>
        /// <param name="filter">A predicate to filter the <paramref name="list"/> collection.</param>
        public ViewsCollection(ObservableCollection<ItemMetadata> list, Predicate<ItemMetadata> filter)
        {
            _subjectCollection = list;
            _filter = filter;
            MonitorAllMetadataItems();
            _subjectCollection.CollectionChanged += SourceCollectionChanged;
            UpdateFilteredItemsList();
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Gets or sets the comparison used to sort the views.
        /// </summary>
        /// <value>The comparison to use.</value>
        public Comparison<object> SortComparison
        {
            get { return _sort; }
            set
            {
                if (_sort != value)
                {
                    _sort = value;
                    UpdateFilteredItemsList();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="value">The object to locate in the collection.</param>
        /// <returns><see langword="true" /> if <paramref name="value"/> is found in the collection; otherwise, <see langword="false" />.</returns>
        public bool Contains(object value)
        {
            return _filteredItems.Contains(value);
        }

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        ///</returns>
        public IEnumerator<object> GetEnumerator()
        {
            return _filteredItems.GetEnumerator();
        }

        ///<summary>
        ///Returns an enumerator that iterates through a collection.
        ///</summary>
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        ///</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Used to invoked the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e"></param>
        void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        void NotifyReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Removes all monitoring of underlying MetadataItems and re-adds them.
        /// </summary>
        void ResetAllMonitors()
        {
            RemoveAllMetadataMonitors();
            MonitorAllMetadataItems();
        }

        /// <summary>
        /// Adds all underlying MetadataItems to the list from the subjectCollection
        /// </summary>
        void MonitorAllMetadataItems()
        {
            foreach (var item in _subjectCollection)
            {
                AddMetadataMonitor(item, _filter(item));
            }
        }

        /// <summary>
        /// Removes all monitored items from our monitoring list.
        /// </summary>
        void RemoveAllMetadataMonitors()
        {
            foreach (var item in _monitoredItems)
            {
                item.Key.MetadataChanged -= OnItemMetadataChanged;
            }

            _monitoredItems.Clear();
        }

        /// <summary>
        /// Adds handler to monitor the MetadatItem and adds it to our monitoring list.
        /// </summary>
        /// <param name="itemMetadata"></param>
        /// <param name="isInList"></param>
        void AddMetadataMonitor(ItemMetadata itemMetadata, bool isInList)
        {
            itemMetadata.MetadataChanged += OnItemMetadataChanged;
            _monitoredItems.Add(
                itemMetadata,
                new MonitorInfo { InList = isInList });
        }

        /// <summary>
        /// Unhooks from the MetadataItem change event and removes from our monitoring list.
        /// </summary>
        /// <param name="itemMetadata"></param>
        void RemoveMetadataMonitor(ItemMetadata itemMetadata)
        {
            itemMetadata.MetadataChanged -= OnItemMetadataChanged;
            _monitoredItems.Remove(itemMetadata);
        }

        /// <summary>
        /// Invoked when any of the underlying ItemMetadata items we're monitoring changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnItemMetadataChanged(object sender, EventArgs e)
        {
            var itemMetadata = (ItemMetadata)sender;

            // Our monitored item may have been removed during another event before
            // our OnItemMetadataChanged got called back, so it's not unexpected
            // that we may not have it in our list.
            var foundInfo = _monitoredItems.TryGetValue(itemMetadata, out MonitorInfo monitorInfo);
            if (!foundInfo) return;
            if (_filter(itemMetadata))
            {
                if (!monitorInfo.InList)
                {
                    // This passes our filter and wasn't marked
                    // as in our list so we can consider this
                    // an Add.
                    monitorInfo.InList = true;
                    UpdateFilteredItemsList();
                    NotifyAdd(itemMetadata.Item);
                }
            }
            else
            {
                // This doesn't fit our filter, we remove from our
                // tracking list, but should not remove any monitoring in
                // case it fits our filter in the future.
                monitorInfo.InList = false;
                RemoveFromFilteredList(itemMetadata.Item);
            }
        }

        /// <summary>
        /// The event handler due to changes in the underlying collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    UpdateFilteredItemsList();
                    foreach (ItemMetadata itemMetadata in e.NewItems)
                    {
                        bool isInFilter = _filter(itemMetadata);
                        AddMetadataMonitor(itemMetadata, isInFilter);
                        if (isInFilter)
                        {
                            NotifyAdd(itemMetadata.Item);
                        }
                    }

                    // If we're sorting we can't predict how
                    // the collection has changed on an add so we 
                    // resort to a reset notification.
                    if (_sort != null)
                    {
                        NotifyReset();
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ItemMetadata itemMetadata in e.OldItems)
                    {
                        RemoveMetadataMonitor(itemMetadata);
                        if (_filter(itemMetadata))
                        {
                            RemoveFromFilteredList(itemMetadata.Item);
                        }
                    }

                    break;
                default:
                    ResetAllMonitors();
                    UpdateFilteredItemsList();
                    NotifyReset();

                    break;
            }
        }

        void NotifyAdd(object item)
        {
            int newIndex = _filteredItems.IndexOf(item);
            NotifyAdd(new[] { item }, newIndex);
        }

        void RemoveFromFilteredList(object item)
        {
            int index = _filteredItems.IndexOf(item);
            UpdateFilteredItemsList();
            NotifyRemove(new[] { item }, index);
        }

        void UpdateFilteredItemsList()
        {
            _filteredItems = _subjectCollection.Where(i => _filter(i))
                .Select(i => i.Item)
                .OrderBy<object, object>(o => o, new RegionItemComparer(SortComparison))
                .ToList();
        }

        class MonitorInfo
        {
            public bool InList { get; set; }
        }

        class RegionItemComparer : Comparer<object>
        {
            readonly Comparison<object> _comparer;

            public RegionItemComparer(Comparison<object> comparer)
            {
                _comparer = comparer;
            }

            public override int Compare(object x, object y)
            {
                if (_comparer == null)
                {
                    return 0;
                }

                return _comparer(x, y);
            }
        }

        void NotifyAdd(IList items, int newStartingIndex)
        {
            if (items.Count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Add,
                                            items,
                                            newStartingIndex));
            }
        }

        void NotifyRemove(IList items, int originalIndex)
        {
            if (items.Count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    items,
                    originalIndex));
            }
        }
    }
}