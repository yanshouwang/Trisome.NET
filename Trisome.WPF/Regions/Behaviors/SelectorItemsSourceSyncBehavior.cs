using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Trisome.WPF.Extension;

#if HAS_WINUI
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#endif

namespace Trisome.WPF.Regions.Behaviors
{
    /// <summary>
    /// Defines the attached behavior that keeps the items of the <see cref="Selector"/> host control in synchronization with the <see cref="IRegion"/>.
    /// 
    /// This behavior also makes sure that, if you activate a view in a region, the SelectedItem is set. If you set the SelectedItem or SelectedItems (ListBox)
    /// then this behavior will also call Activate on the selected items. 
    /// <remarks>
    /// When calling Activate on a view, you can only select a single active view at a time. By setting the SelectedItems property of a listbox, you can set
    /// multiple views to active. 
    /// </remarks>
    /// </summary>
    public class SelectorItemsSourceSyncBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
        /// <summary>
        /// Name that identifies the SelectorItemsSourceSyncBehavior behavior in a collection of RegionsBehaviors. 
        /// </summary>
        public static readonly string KEY = "SelectorItemsSourceSyncBehavior";

        bool _updatingActiveViewsInHostControlSelectionChanged;
        Selector _hostControl;

        /// <summary>
        /// Gets or sets the <see cref="DependencyObject"/> that the <see cref="IRegion"/> is attached to.
        /// </summary>
        /// <value>
        /// A <see cref="DependencyObject"/> that the <see cref="IRegion"/> is attached to.
        /// </value>
        /// <remarks>For this behavior, the host control must always be a <see cref="Selector"/> or an inherited class.</remarks>
        public DependencyObject HostControl
        {
            get
            {
                return _hostControl;
            }

            set
            {
                _hostControl = value as Selector;
            }
        }

        /// <summary>
        /// Starts to monitor the <see cref="IRegion"/> to keep it in synch with the items of the <see cref="HostControl"/>.
        /// </summary>
        protected override void OnAttach()
        {
            bool itemsSourceIsSet = _hostControl.ItemsSource != null;
            itemsSourceIsSet = itemsSourceIsSet || _hostControl.HasBinding(ItemsControl.ItemsSourceProperty);

            if (itemsSourceIsSet)
            {
                throw new InvalidOperationException("ItemsControl's ItemsSource property is not empty. This control is being associated with a region, but the control is already bound to something else. If you did not explicitly set the control's ItemSource property, this exception may be caused by a change in the value of the inherited RegionManager attached property.");
            }

            SynchronizeItems();

            _hostControl.SelectionChanged += HostControlSelectionChanged;
            Region.ActiveViews.CollectionChanged += ActiveViews_CollectionChanged;
            Region.Views.CollectionChanged += Views_CollectionChanged;
        }

        void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                int startIndex = e.NewStartingIndex;
                foreach (object newItem in e.NewItems)
                {
                    _hostControl.Items.Insert(startIndex++, newItem);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object oldItem in e.OldItems)
                {
                    _hostControl.Items.Remove(oldItem);
                }
            }
        }

        void SynchronizeItems()
        {
            var existingItems = new List<object>();

            // Control must be empty before "Binding" to a region
            foreach (object childItem in _hostControl.Items)
            {
                existingItems.Add(childItem);
            }

            foreach (object view in Region.Views)
            {
                _hostControl.Items.Add(view);
            }

            foreach (object existingItem in existingItems)
            {
                Region.Add(existingItem);
            }
        }

        void ActiveViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updatingActiveViewsInHostControlSelectionChanged)
            {
                // If we are updating the ActiveViews collection in the HostControlSelectionChanged, that 
                // means the user has set the SelectedItem or SelectedItems himself and we don't need to do that here now
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (_hostControl.SelectedItem != null
                    && _hostControl.SelectedItem != e.NewItems[0]
                    && Region.ActiveViews.Contains(_hostControl.SelectedItem))
                {
                    Region.Deactivate(_hostControl.SelectedItem);
                }

                _hostControl.SelectedItem = e.NewItems[0];
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove &&
                     e.OldItems.Contains(_hostControl.SelectedItem))
            {
                _hostControl.SelectedItem = null;
            }
        }

        void HostControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Record the fact that we are now updating active views in the HostControlSelectionChanged method. 
                // This is needed to prevent the ActiveViews_CollectionChanged() method from firing. 
                _updatingActiveViewsInHostControlSelectionChanged = true;

                object source;
                source = e.OriginalSource;

                if (source == sender)
                {
                    foreach (object item in e.RemovedItems)
                    {
                        // check if the view is in both Views and ActiveViews collections (there may be out of sync)
                        if (Region.Views.Contains(item) && Region.ActiveViews.Contains(item))
                        {
                            Region.Deactivate(item);
                        }
                    }

                    foreach (object item in e.AddedItems)
                    {
                        if (Region.Views.Contains(item) && !Region.ActiveViews.Contains(item))
                        {
                            Region.Activate(item);
                        }
                    }
                }
            }
            finally
            {
                _updatingActiveViewsInHostControlSelectionChanged = false;
            }
        }
    }
}
