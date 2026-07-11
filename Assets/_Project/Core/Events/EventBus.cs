using System;
using System.Collections.Generic;

namespace BookLab.Core.Events
{
    // Tiny publish/subscribe "dinner bell". Parts raise events; others listen,
    // without referencing each other directly — this is the event-driven requirement.
    public static class EventBus
    {
        static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler)
        {
            _handlers.TryGetValue(typeof(T), out var d);
            _handlers[typeof(T)] = Delegate.Combine(d, handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (_handlers.TryGetValue(typeof(T), out var d))
                _handlers[typeof(T)] = Delegate.Remove(d, handler);
        }

        public static void Publish<T>(T evt)
        {
            if (_handlers.TryGetValue(typeof(T), out var d))
                (d as Action<T>)?.Invoke(evt);
        }
    }
}
