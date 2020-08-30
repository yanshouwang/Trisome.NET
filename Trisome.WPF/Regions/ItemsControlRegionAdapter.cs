using System;
using Trisome.WPF.Extension;

#if HAS_WINUI
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Controls;
#endif

namespace Trisome.WPF.Regions
{
    /// <summary>
    /// Adapter that creates a new <see cref="AllActiveRegion"/> and binds all
    /// the views to the adapted <see cref="ItemsControl"/>.
    /// </summary>
    public class ItemsControlRegionAdapter : BaseRegionAdapter<ItemsControl>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ItemsControlRegionAdapter"/>.
        /// </summary>
        /// <param name="regionBehaviorFactory">The factory used to create the region behaviors to attach to the created regions.</param>
        public ItemsControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        /// <summary>
        /// Adapts an <see cref="ItemsControl"/> to an <see cref="IRegion"/>.
        /// </summary>
        /// <param name="region">The new region being used.</param>
        /// <param name="regionTarget">The object to adapt.</param>
        protected override void Adapt(IRegion region, ItemsControl regionTarget)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            bool itemsSourceIsSet = regionTarget.ItemsSource != null;
            itemsSourceIsSet = itemsSourceIsSet || regionTarget.HasBinding(ItemsControl.ItemsSourceProperty);

            if (itemsSourceIsSet)
            {
                throw new InvalidOperationException("ItemsControl's ItemsSource property is not empty. This control is being associated with a region, but the control is already bound to something else. If you did not explicitly set the control's ItemSource property, this exception may be caused by a change in the value of the inherited RegionManager attached property.");
            }

            // If control has child items, move them to the region and then bind control to region. Can't set ItemsSource if child items exist.
            if (regionTarget.Items.Count > 0)
            {
                foreach (object childItem in regionTarget.Items)
                {
                    region.Add(childItem);
                }
                // Control must be empty before setting ItemsSource
                regionTarget.Items.Clear();
            }

            regionTarget.ItemsSource = region.Views;
        }

        /// <summary>
        /// Creates a new instance of <see cref="AllActiveRegion"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="AllActiveRegion"/>.</returns>
        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
