using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Trisome.WPF.Common;

#if HAS_WINUI
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Trisome.WPF.Regions.Behaviors
{
    /// <summary>
    /// Defines a behavior that forwards the <see cref="RegionManager.RegionContextProperty"/> 
    /// to the views in the region.
    /// </summary>
    public class BindRegionContextToDependencyObjectBehavior : IRegionBehavior
    {
        /// <summary>
        /// The key of this behavior.
        /// </summary>
        public const string KEY = "ContextToDependencyObject";

        /// <summary>
        /// Behavior's attached region.
        /// </summary>
        public IRegion Region { get; set; }

        /// <summary>
        /// Attaches the behavior to the specified region.
        /// </summary>
        public void Attach()
        {
            Region.Views.CollectionChanged += Views_CollectionChanged;
            Region.PropertyChanged += Region_PropertyChanged;
            SetContextToViews(Region.Views, Region.Context);
            AttachNotifyChangeEvent(Region.Views);
        }

        static void SetContextToViews(IEnumerable views, object context)
        {
            foreach (var view in views)
            {
                if (view is DependencyObject dependencyObjectView)
                {
                    var contextWrapper = RegionContext.GetObservableContext(dependencyObjectView);
                    contextWrapper.Value = context;
                }
            }
        }

        void AttachNotifyChangeEvent(IEnumerable views)
        {
            foreach (var view in views)
            {
                if (view is DependencyObject dependencyObject)
                {
                    var viewRegionContext = RegionContext.GetObservableContext(dependencyObject);
                    viewRegionContext.PropertyChanged += this.ViewRegionContext_OnPropertyChangedEvent;
                }
            }
        }

        void DetachNotifyChangeEvent(IEnumerable views)
        {
            foreach (var view in views)
            {
                if (view is DependencyObject dependencyObject)
                {
                    var viewRegionContext = RegionContext.GetObservableContext(dependencyObject);
                    viewRegionContext.PropertyChanged -= this.ViewRegionContext_OnPropertyChangedEvent;
                }
            }
        }

        void ViewRegionContext_OnPropertyChangedEvent(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Value")
            {
                var context = (ObservableObject<object>)sender;
                Region.Context = context.Value;
            }
        }

        void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SetContextToViews(e.NewItems, this.Region.Context);
                AttachNotifyChangeEvent(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && this.Region.Context != null)
            {
                DetachNotifyChangeEvent(e.OldItems);
                SetContextToViews(e.OldItems, null);
            }
        }

        void Region_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Context")
            {
                SetContextToViews(this.Region.Views, this.Region.Context);
            }
        }
    }
}
