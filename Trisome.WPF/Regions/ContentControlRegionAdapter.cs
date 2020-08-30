using System;
using System.Collections.Specialized;
using System.Linq;
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
    /// Adapter that creates a new <see cref="SingleActiveRegion"/> and monitors its
    /// active view to set it on the adapted <see cref="ContentControl"/>.
    /// </summary>
    public class ContentControlRegionAdapter : BaseRegionAdapter<ContentControl>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ContentControlRegionAdapter"/>.
        /// </summary>
        /// <param name="regionBehaviorFactory">The factory used to create the region behaviors to attach to the created regions.</param>
        public ContentControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        /// <summary>
        /// Adapts a <see cref="ContentControl"/> to an <see cref="IRegion"/>.
        /// </summary>
        /// <param name="region">The new region being used.</param>
        /// <param name="regionTarget">The object to adapt.</param>
        protected override void Adapt(IRegion region, ContentControl regionTarget)
        {
            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            bool contentIsSet = regionTarget.Content != null;
            contentIsSet = contentIsSet || regionTarget.HasBinding(ContentControl.ContentProperty);

            if (contentIsSet)
                throw new InvalidOperationException("ContentControl's Content property is not empty. This control is being associated with a region, but the control is already bound to something else. If you did not explicitly set the control's Content property, this exception may be caused by a change in the value of the inherited RegionManager attached property.");

            region.ActiveViews.CollectionChanged += delegate
            {
                regionTarget.Content = region.ActiveViews.FirstOrDefault();
            };

            region.Views.CollectionChanged +=
                (sender, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add && region.ActiveViews.Count() == 0)
                    {
                        region.Activate(e.NewItems[0]);
                    }
                };
        }

        /// <summary>
        /// Creates a new instance of <see cref="SingleActiveRegion"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="SingleActiveRegion"/>.</returns>
        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }
    }
}
