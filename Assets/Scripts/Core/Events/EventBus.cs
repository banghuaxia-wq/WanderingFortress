using System;
using System.Collections.Generic;

namespace WF.Gameplay.Core.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
            {
                _subscribers[type] = new List<Delegate>();
            }
            if (!_subscribers[type].Contains(handler))
            {
                _subscribers[type].Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Remove(handler);
            }
        }

        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                var handlersCopy = new List<Delegate>(handlers);
                foreach (var handler in handlersCopy)
                {
                    (handler as Action<T>)?.Invoke(eventData);
                }
            }
        }
    }
}
