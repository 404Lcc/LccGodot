using System;
using System.Collections.Generic;
using Godot;

namespace LccHotfix
{
    internal sealed class ValueEventManager : Module, IValueEventService
    {
        private readonly Dictionary<Type, HashSet<Delegate>> _handlers = new();

        public void AddHandle<T>(Action<T> handle) where T : struct, IValueEvent
        {
            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var list))
            {
                list = new HashSet<Delegate>();
                _handlers[eventType] = list;
            }

            if (!list.Add(handle))
            {
                GD.PrintErr($"Duplicate value event handler: {eventType.FullName}");
            }
        }

        public void RemoveHandle<T>(Action<T> handle) where T : struct, IValueEvent
        {
            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var list))
            {
                return;
            }

            list.Remove(handle);
            if (list.Count == 0)
            {
                _handlers.Remove(eventType);
            }
        }

        public void Dispatch<T>(T value) where T : struct, IValueEvent
        {
            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var list))
            {
                return;
            }

            foreach (var handler in list)
            {
                ((Action<T>)handler).Invoke(value);
            }
        }

        internal override void Shutdown()
        {
            _handlers.Clear();
        }
    }
}
