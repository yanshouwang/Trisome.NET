using System;

namespace Trisome.WPF.Regions.Behaviors
{
    /// <summary>
    /// Provides a base class for region's behaviors.
    /// </summary>
    public abstract class RegionBehavior : IRegionBehavior
    {
        private IRegion _region;

        /// <summary>
        /// Behavior's attached region.
        /// </summary>
        public IRegion Region
        {
            get
            {
                return _region;
            }
            set
            {
                if (Attached)
                {
                    throw new InvalidOperationException("The Region property cannot be set after Attach method has been called.");
                }

                _region = value;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the behavior is attached to a region, <see langword="false"/> otherwise.
        /// </summary>
        public bool Attached { get; private set; }

        /// <summary>
        /// Attaches the behavior to the region.
        /// </summary>
        public void Attach()
        {
            if (_region == null)
            {
                throw new InvalidOperationException("The Attach method cannot be called when Region property is null.");
            }

            Attached = true;
            OnAttach();
        }

        /// <summary>
        /// Override this method to perform the logic after the behavior has been attached.
        /// </summary>
        protected abstract void OnAttach();
    }
}