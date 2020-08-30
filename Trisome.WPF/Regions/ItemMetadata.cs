using System;

#if HAS_WINUI
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Trisome.WPF.Regions
{
    /// <summary>
    /// Defines a class that wraps an item and adds metadata for it.
    /// </summary>
    public class ItemMetadata : DependencyObject
    {
        /// <summary>
        /// Occurs when metadata on the item changes.
        /// </summary>
        public event EventHandler MetadataChanged;

        /// <summary>
        /// Gets the wrapped item.
        /// </summary>
        /// <value>The wrapped item.</value>
        public object Item { get; private set; }

        /// <summary>
        /// Gets or sets a name for the wrapped item.
        /// </summary>
        /// <value>The name of the wrapped item.</value>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// The name of the wrapped item.
        /// </summary>
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ItemMetadata), null);

        /// <summary>
        /// Gets or sets a value indicating whether the wrapped item is considered active.
        /// </summary>
        /// <value><see langword="true" /> if the item should be considered active; otherwise <see langword="false" />.</value>
        public bool Active
        {
            get { return (bool)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); }
        }

        /// <summary>
        /// Value indicating whether the wrapped item is considered active.
        /// </summary>
        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register("Active", typeof(bool), typeof(ItemMetadata), new PropertyMetadata(defaultValue: false, propertyChangedCallback: DependencyPropertyChanged));

        /// <summary>
        /// Initializes a new instance of <see cref="ItemMetadata"/>.
        /// </summary>
        /// <param name="item">The item to wrap.</param>
        public ItemMetadata(object item)
        {
            // check for null
            Item = item;
        }

        /// <summary>
        /// Explicitly invokes <see cref="MetadataChanged"/> to notify listeners.
        /// </summary>
        public void InvokeMetadataChanged()
        {
            MetadataChanged?.Invoke(this, EventArgs.Empty);
        }

        static void DependencyPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is ItemMetadata itemMetadata)
            {
                itemMetadata.InvokeMetadataChanged();
            }
        }
    }
}