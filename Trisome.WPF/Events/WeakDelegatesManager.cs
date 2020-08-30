using System;
using System.Collections.Generic;
using System.Linq;
using Trisome.Core.Events;

namespace Trisome.WPF.Events
{
    class WeakDelegatesManager
    {
        readonly List<DelegateReference> _listeners;

        public WeakDelegatesManager()
        {
            _listeners = new List<DelegateReference>();
        }

        public void AddListener(Delegate listener)
        {
            _listeners.Add(new DelegateReference(listener, false));
        }

        public void RemoveListener(Delegate listener)
        {
            //Remove the listener, and prune collected listeners
            _listeners.RemoveAll(reference => reference.TargetEquals(null) || reference.TargetEquals(listener));
        }

        public void Raise(params object[] args)
        {
            _listeners.RemoveAll(listener => listener.TargetEquals(null));

            foreach (var handler in _listeners.Select(listener => listener.Target).Where(listener => listener != null).ToList())
            {
                handler.DynamicInvoke(args);
            }
        }
    }
}
