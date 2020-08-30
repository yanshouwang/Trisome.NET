using System;
using System.Collections.Generic;
using System.Threading;

namespace Trisome.Core.Events
{
    /// <summary>
    /// Implements <see cref="IEventAggregator"/>.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        readonly Dictionary<Type, BaseEvent> _events;
        // Captures the sync context for the UI thread when constructed on the UI thread 
        // in a platform agnositc way so it can be used for UI thread dispatching
        readonly SynchronizationContext _syncContext;

        public EventAggregator()
        {
            _events = new Dictionary<Type, BaseEvent>();
            _syncContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Gets the single instance of the event managed by this EventAggregator. Multiple calls to this method with the same <typeparamref name="TEventType"/> returns the same event instance.
        /// </summary>
        /// <typeparam name="TEventType">The type of event to get. This must inherit from <see cref="EventBase"/>.</typeparam>
        /// <returns>A singleton instance of an event object of type <typeparamref name="TEventType"/>.</returns>
        public TEventType GetEvent<TEventType>() where TEventType : BaseEvent, new()
        {
            lock (_events)
            {
                if (!_events.TryGetValue(typeof(TEventType), out BaseEvent existingEvent))
                {
                    TEventType newEvent = new TEventType();
                    newEvent.SynchronizationContext = _syncContext;
                    _events[typeof(TEventType)] = newEvent;

                    return newEvent;
                }
                else
                {
                    return (TEventType)existingEvent;
                }
            }
        }
    }
}
