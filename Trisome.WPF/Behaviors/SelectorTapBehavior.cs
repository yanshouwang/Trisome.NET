using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Trisome.WPF.Behaviors
{
    public class SelectorTapBehavior : Behavior<Selector>
    {
        object _item;

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(SelectorTapBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp += OnMouseUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp -= OnMouseUp;

            base.OnDetaching();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject.SelectedItem == null)
                return;
            if (_item == null)
            {
                _item = AssociatedObject.SelectedItem;
            }
            else
            {
                AssociatedObject.SelectedItem = null;
            }
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            var item = AssociatedObject.SelectedItem;
            if (item == null)
                return;
            var container = (FrameworkElement)AssociatedObject.ItemContainerGenerator.ContainerFromItem(item);
            var location = e.GetPosition(container);
            var bounds = new Rect(0, 0, container.ActualWidth, container.ActualHeight);
            if (bounds.Contains(location))
                return;
            AssociatedObject.SelectedItem = null;
        }

        void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = AssociatedObject.SelectedItem;
            if (item != null)
            {
                if (ClickCommand != null && ClickCommand.CanExecute(item))
                {
                    ClickCommand.Execute(item);
                }
                AssociatedObject.SelectedItem = null;
            }
            _item = null;
        }
    }
}
