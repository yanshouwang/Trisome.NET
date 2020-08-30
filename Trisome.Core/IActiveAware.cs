using System;

namespace Trisome.Core
{
    /// <summary>
    /// Interface that defines if the object instance is active and notifies when the activity changes.
    /// </summary>
    public interface IActiveAware
    {
        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value><see langword="true" /> if the object is active; otherwise <see langword="false" />.</value>
        bool Active { get; set; }

        /// <summary>
        /// Notifies that the value for <see cref="Active"/> property has changed.
        /// </summary>
        event EventHandler ActiveChanged;
    }
}
