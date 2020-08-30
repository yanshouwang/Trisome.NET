﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Trisome.Core.IoC;
using System.Collections.Generic;

#if HAS_WINUI
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
#endif

namespace Trisome.WPF.Regions
{
    /// <summary>
    /// Implementation of <see cref="IRegion"/> that allows multiple active views.
    /// </summary>
    public class Region : IRegion
    {
        ObservableCollection<ItemMetadata> _itemMetadataCollection;
        string _name;
        ViewsCollection _views;
        ViewsCollection _activeViews;
        object _context;
        IRegionManager _regionManager;
        IRegionNavigationService _regionNavigationService;

        Comparison<object> _sort;

        /// <summary>
        /// Initializes a new instance of <see cref="Region"/>.
        /// </summary>
        public Region()
        {
            Behaviors = new RegionBehaviorCollection(this);

            _sort = DefaultSortComparison;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the collection of <see cref="IRegionBehavior"/>s that can extend the behavior of regions.
        /// </summary>
        public IRegionBehaviorCollection Behaviors { get; set; }

        /// <summary>
        /// Gets or sets a context for the region. This value can be used by the user to share context with the views.
        /// </summary>
        /// <value>The context value to be shared.</value>
        public object Context
        {
            get => _context;

            set
            {
                if (_context != value)
                {
                    _context = value;
                    OnPropertyChanged(nameof(Context));
                }
            }
        }

        /// <summary>
        /// Gets the name of the region that uniequely identifies the region within a <see cref="IRegionManager"/>.
        /// </summary>
        /// <value>The name of the region.</value>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != null && _name != value)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        "Cannot change the region name once is set. The current region name is '{0}'.",
                        _name));
                }

                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("The region name cannot be null or empty.");
                }

                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets a readonly view of the collection of views in the region.
        /// </summary>
        /// <value>An <see cref="IViewsCollection"/> of all the added views.</value>
        public virtual IViewsCollection Views
        {
            get
            {
                if (_views == null)
                {
                    _views = new ViewsCollection(ItemMetadataCollection, x => true)
                    {
                        SortComparison = _sort
                    };
                }

                return _views;
            }
        }

        /// <summary>
        /// Gets a readonly view of the collection of all the active views in the region.
        /// </summary>
        /// <value>An <see cref="IViewsCollection"/> of all the active views.</value>
        public virtual IViewsCollection ActiveViews
        {
            get
            {
                if (_views == null)
                {
                    _views = new ViewsCollection(ItemMetadataCollection, x => true)
                    {
                        SortComparison = _sort
                    };
                }

                if (_activeViews == null)
                {
                    _activeViews = new ViewsCollection(ItemMetadataCollection, x => x.Active)
                    {
                        SortComparison = _sort
                    };
                }

                return _activeViews;
            }
        }

        /// <summary>
        /// Gets or sets the comparison used to sort the views.
        /// </summary>
        /// <value>The comparison to use.</value>
        public Comparison<object> SortComparison
        {
            get => _sort;
            set
            {
                _sort = value;

                if (_activeViews != null)
                {
                    _activeViews.SortComparison = _sort;
                }

                if (_views != null)
                {
                    _views.SortComparison = _sort;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IRegionManager"/> that will be passed to the views when adding them to the region, unless the view is added by specifying createRegionManagerScope as <see langword="true" />.
        /// </summary>
        /// <value>The <see cref="IRegionManager"/> where this <see cref="IRegion"/> is registered.</value>
        /// <remarks>This is usually used by implementations of <see cref="IRegionManager"/> and should not be
        /// used by the developer explicitely.</remarks>
        public IRegionManager RegionManager
        {
            get => _regionManager;

            set
            {
                if (_regionManager != value)
                {
                    _regionManager = value;
                    OnPropertyChanged(nameof(RegionManager));
                }
            }
        }

        /// <summary>
        /// Gets the navigation service.
        /// </summary>
        /// <value>The navigation service.</value>
        public IRegionNavigationService NavigationService
        {
            get
            {
                if (_regionNavigationService == null)
                {
                    _regionNavigationService = ContainerLocator.Container.Resolve<IRegionNavigationService>();
                    _regionNavigationService.Region = this;
                }

                return _regionNavigationService;
            }

            set => _regionNavigationService = value;
        }

        /// <summary>
        /// Gets the collection with all the views along with their metadata.
        /// </summary>
        /// <value>An <see cref="ObservableCollection{T}"/> of <see cref="ItemMetadata"/> with all the added views.</value>
        protected virtual ObservableCollection<ItemMetadata> ItemMetadataCollection
        {
            get
            {
                if (_itemMetadataCollection == null)
                {
                    _itemMetadataCollection = new ObservableCollection<ItemMetadata>();
                }

                return _itemMetadataCollection;
            }
        }

        /// <overloads>Adds a new view to the region.</overloads>
        /// <summary>
        /// Adds a new view to the region.
        /// </summary>
        /// <param name="view">The view to add.</param>
        /// <returns>The <see cref="IRegionManager"/> that is set on the view if it is a <see cref="DependencyObject"/>. It will be the current region manager when using this overload.</returns>
        public IRegionManager Add(object view)
        {
            return Add(view, null, false);
        }

        /// <summary>
        /// Adds a new view to the region.
        /// </summary>
        /// <param name="view">The view to add.</param>
        /// <param name="viewName">The name of the view. This can be used to retrieve it later by calling <see cref="IRegion.GetView"/>.</param>
        /// <returns>The <see cref="IRegionManager"/> that is set on the view if it is a <see cref="DependencyObject"/>. It will be the current region manager when using this overload.</returns>
        public IRegionManager Add(object view, string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The provided String argument {0} must not be null or empty.",
                    "viewName"));
            }

            return Add(view, viewName, false);
        }

        /// <summary>
        /// Adds a new view to the region.
        /// </summary>
        /// <param name="view">The view to add.</param>
        /// <param name="viewName">The name of the view. This can be used to retrieve it later by calling <see cref="IRegion.GetView"/>.</param>
        /// <param name="createRegionManagerScope">When <see langword="true"/>, the added view will receive a new instance of <see cref="IRegionManager"/>, otherwise it will use the current region manager for this region.</param>
        /// <returns>The <see cref="IRegionManager"/> that is set on the view if it is a <see cref="DependencyObject"/>.</returns>
        public virtual IRegionManager Add(object view, string viewName, bool createRegionManagerScope)
        {
            IRegionManager manager = createRegionManagerScope ? RegionManager.CreateRegionManager() : RegionManager;
            InnerAdd(view, viewName, manager);
            return manager;
        }

        /// <summary>
        /// Removes the specified view from the region.
        /// </summary>
        /// <param name="view">The view to remove.</param>
        public virtual void Remove(object view)
        {
            ItemMetadata itemMetadata = GetItemMetadataOrThrow(view);

            ItemMetadataCollection.Remove(itemMetadata);

            if (view is DependencyObject dependencyObject && Regions.RegionManager.GetRegionManager(dependencyObject) == RegionManager)
            {
                dependencyObject.ClearValue(Regions.RegionManager.RegionManagerProperty);
            }
        }

        /// <summary>
        /// Removes all views from the region.
        /// </summary>
        public void RemoveAll()
        {
            foreach (var view in Views)
            {
                Remove(view);
            }
        }

        /// <summary>
        /// Marks the specified view as active.
        /// </summary>
        /// <param name="view">The view to activate.</param>
        public virtual void Activate(object view)
        {
            ItemMetadata itemMetadata = GetItemMetadataOrThrow(view);

            if (!itemMetadata.Active)
            {
                itemMetadata.Active = true;
            }
        }

        /// <summary>
        /// Marks the specified view as inactive.
        /// </summary>
        /// <param name="view">The view to deactivate.</param>
        public virtual void Deactivate(object view)
        {
            ItemMetadata itemMetadata = GetItemMetadataOrThrow(view);

            if (itemMetadata.Active)
            {
                itemMetadata.Active = false;
            }
        }

        /// <summary>
        /// Returns the view instance that was added to the region using a specific name.
        /// </summary>
        /// <param name="viewName">The name used when adding the view to the region.</param>
        /// <returns>Returns the named view or <see langword="null"/> if the view with <paramref name="viewName"/> does not exist in the current region.</returns>
        public virtual object GetView(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The provided String argument {0} must not be null or empty.",
                    "viewName"));
            }

            ItemMetadata metadata = ItemMetadataCollection.FirstOrDefault(x => x.Name == viewName);

            if (metadata != null)
            {
                return metadata.Item;
            }

            return null;
        }

        /// <summary>
        /// Initiates navigation to the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="navigationCallback">A callback to execute when the navigation request is completed.</param>
        public void Navigate(Uri target, Action<NavigationResult> navigationCallback)
        {
            Navigate(target, navigationCallback, null);
        }

        /// <summary>
        /// Initiates navigation to the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="navigationCallback">A callback to execute when the navigation request is completed.</param>
        /// <param name="navigationParameters">The navigation parameters specific to the navigation request.</param>
        public void Navigate(Uri target, Action<NavigationResult> navigationCallback, IDictionary<string,object> args)
        {
            NavigationService.Navigate(target, navigationCallback, args);
        }

        void InnerAdd(object view, string viewName, IRegionManager scopedRegionManager)
        {
            if (ItemMetadataCollection.FirstOrDefault(x => x.Item == view) != null)
            {
                throw new InvalidOperationException("View already exists in region.");
            }

            ItemMetadata itemMetadata = new ItemMetadata(view);
            if (!string.IsNullOrEmpty(viewName))
            {
                if (ItemMetadataCollection.FirstOrDefault(x => x.Name == viewName) != null)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "View with name '{0}' already exists in the region.",
                        viewName));
                }
                itemMetadata.Name = viewName;
            }


            if (view is DependencyObject dependencyObject)
            {
                Regions.RegionManager.SetRegionManager(dependencyObject, scopedRegionManager);
            }

            ItemMetadataCollection.Add(itemMetadata);
        }

        ItemMetadata GetItemMetadataOrThrow(object view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            ItemMetadata itemMetadata = ItemMetadataCollection.FirstOrDefault(x => x.Item == view);

            if (itemMetadata == null)
                throw new ArgumentException("The region does not contain the specified view.", nameof(view));

            return itemMetadata;
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The default sort algorithm.
        /// </summary>
        /// <param name="x">The first view to compare.</param>
        /// <param name="y">The second view to compare.</param>
        /// <returns></returns>
        public static int DefaultSortComparison(object x, object y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    Type xType = x.GetType();
                    Type yType = y.GetType();

                    ViewSortHintAttribute xAttribute = xType.GetCustomAttributes(typeof(ViewSortHintAttribute), true).FirstOrDefault() as ViewSortHintAttribute;
                    ViewSortHintAttribute yAttribute = yType.GetCustomAttributes(typeof(ViewSortHintAttribute), true).FirstOrDefault() as ViewSortHintAttribute;

                    return ViewSortHintAttributeComparison(xAttribute, yAttribute);
                }
            }
        }

        static int ViewSortHintAttributeComparison(ViewSortHintAttribute x, ViewSortHintAttribute y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    return string.Compare(x.Hint, y.Hint, StringComparison.Ordinal);
                }
            }
        }
    }
}