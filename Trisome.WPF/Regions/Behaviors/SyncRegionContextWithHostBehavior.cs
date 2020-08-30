using System;
using Trisome.WPF.Common;

#if HAS_WINUI
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Trisome.WPF.Regions.Behaviors
{
    /// <summary>
    /// Behavior that synchronizes the <see cref="IRegion.Context"/> property of a <see cref="IRegion"/> with 
    /// the control that hosts the Region. It does this by setting the <see cref="RegionManager.RegionContextProperty"/> 
    /// Dependency Property on the host control.
    /// 
    /// This behavior allows the usage of two way databinding of the RegionContext from XAML. 
    /// </summary>
    public class SyncRegionContextWithHostBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
        const string REGION_CONTEXT_PROPERTY_NAME = "Context";
        DependencyObject _hostControl;

        /// <summary>
        /// Name that identifies the SyncRegionContextWithHostBehavior behavior in a collection of RegionsBehaviors. 
        /// </summary>
        public static readonly string KEY = "SyncRegionContextWithHost";

        ObservableObject<object> HostControlRegionContext
           => RegionContext.GetObservableContext(_hostControl);

        /// <summary>
        /// Gets or sets the <see cref="DependencyObject"/> that the <see cref="IRegion"/> is attached to.
        /// </summary>
        /// <value>
        /// A <see cref="DependencyObject"/> that the <see cref="IRegion"/> is attached to.
        /// This is usually a <see cref="FrameworkElement"/> that is part of the tree.
        /// </value>
        public DependencyObject HostControl
        {
            get
            {
                return _hostControl;
            }
            set
            {
                if (Attached)
                {
                    throw new InvalidOperationException("The HostControl property cannot be set after Attach method has been called.");
                }
                _hostControl = value;
            }
        }

        /// <summary>
        /// Override this method to perform the logic after the behavior has been attached.
        /// </summary>
        protected override void OnAttach()
        {
            if (HostControl != null)
            {
                // Sync values initially. 
                SynchronizeRegionContext();
                // Now register for events to keep them in sync
                HostControlRegionContext.PropertyChanged += this.RegionContextObservableObject_PropertyChanged;
                Region.PropertyChanged += this.Region_PropertyChanged;
            }
        }

        void Region_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == REGION_CONTEXT_PROPERTY_NAME)
            {
                if (RegionManager.GetRegionContext(this.HostControl) != this.Region.Context)
                {
                    // Setting this Dependency Property will automatically also change the HostControlRegionContext.Value
                    // (see RegionManager.OnRegionContextChanged())
                    RegionManager.SetRegionContext(this._hostControl, this.Region.Context);
                }
            }
        }

        void RegionContextObservableObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                SynchronizeRegionContext();
            }
        }

        void SynchronizeRegionContext()
        {
            // Forward this value to the Region
            if (Region.Context != HostControlRegionContext.Value)
            {
                Region.Context = HostControlRegionContext.Value;
            }
            // Also make sure the region's DependencyProperty was changed (this can occur if the value
            // was changed only on the HostControlRegionContext)
            if (RegionManager.GetRegionContext(HostControl) != HostControlRegionContext.Value)
            {
                RegionManager.SetRegionContext(HostControl, HostControlRegionContext.Value);
            }
        }
    }
}
