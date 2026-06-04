using System;
using System.Collections.Generic;
using Godot;

namespace LccGodot.Events;

public sealed class EventGroup
{
    private readonly Dictionary<Type, List<Action<IEventMessage>>> _cachedListeners = new();

    public void AddListener<TEvent>(Action<IEventMessage> listener) where TEvent : IEventMessage
    {
        Type eventType = typeof(TEvent);
        if (!_cachedListeners.TryGetValue(eventType, out List<Action<IEventMessage>>? listeners))
        {
            listeners = new List<Action<IEventMessage>>();
            _cachedListeners.Add(eventType, listeners);
        }

        if (listeners.Contains(listener))
        {
            GD.PushWarning($"Event listener already exists: {eventType.FullName}");
            return;
        }

        listeners.Add(listener);
        Event.AddListener(eventType, listener);
    }

    public void RemoveAllListener()
    {
        foreach ((Type eventType, List<Action<IEventMessage>> listeners) in _cachedListeners)
        {
            foreach (Action<IEventMessage> listener in listeners)
            {
                Event.RemoveListener(eventType, listener);
            }

            listeners.Clear();
        }

        _cachedListeners.Clear();
    }
}
