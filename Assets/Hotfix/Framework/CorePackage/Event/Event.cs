using System;
using System.Collections.Generic;
using Godot;

namespace LccGodot.Events;

public static class Event
{
    private sealed class PostWrapper
    {
        public ulong PostFrame;
        public int EventId;
        public IEventMessage Message = null!;
    }

    private static readonly Dictionary<int, LinkedList<Action<IEventMessage>>> Listeners = new();
    private static readonly List<PostWrapper> PostingList = new();

    public static void Update()
    {
        ulong currentFrame = Engine.GetProcessFrames();
        for (int i = PostingList.Count - 1; i >= 0; i--)
        {
            PostWrapper wrapper = PostingList[i];
            if (currentFrame <= wrapper.PostFrame)
            {
                continue;
            }

            SendMessage(wrapper.EventId, wrapper.Message);
            PostingList.RemoveAt(i);
        }
    }

    public static void ClearAll()
    {
        foreach (LinkedList<Action<IEventMessage>> listenerList in Listeners.Values)
        {
            listenerList.Clear();
        }

        Listeners.Clear();
        PostingList.Clear();
    }

    public static void AddListener<TEvent>(Action<IEventMessage> listener) where TEvent : IEventMessage
    {
        AddListener(typeof(TEvent), listener);
    }

    public static void AddListener(Type eventType, Action<IEventMessage> listener)
    {
        AddListener(eventType.GetHashCode(), listener);
    }

    public static void AddListener(int eventId, Action<IEventMessage> listener)
    {
        if (!Listeners.TryGetValue(eventId, out LinkedList<Action<IEventMessage>>? listenerList))
        {
            listenerList = new LinkedList<Action<IEventMessage>>();
            Listeners.Add(eventId, listenerList);
        }

        if (!listenerList.Contains(listener))
        {
            listenerList.AddLast(listener);
        }
    }

    public static void RemoveListener<TEvent>(Action<IEventMessage> listener) where TEvent : IEventMessage
    {
        RemoveListener(typeof(TEvent), listener);
    }

    public static void RemoveListener(Type eventType, Action<IEventMessage> listener)
    {
        RemoveListener(eventType.GetHashCode(), listener);
    }

    public static void RemoveListener(int eventId, Action<IEventMessage> listener)
    {
        if (Listeners.TryGetValue(eventId, out LinkedList<Action<IEventMessage>>? listenerList))
        {
            listenerList.Remove(listener);
        }
    }

    public static void SendMessage(IEventMessage message)
    {
        SendMessage(message.GetType().GetHashCode(), message);
    }

    public static void SendMessage(int eventId, IEventMessage message)
    {
        if (!Listeners.TryGetValue(eventId, out LinkedList<Action<IEventMessage>>? listenerList))
        {
            return;
        }

        LinkedListNode<Action<IEventMessage>>? currentNode = listenerList.Last;
        while (currentNode != null)
        {
            currentNode.Value.Invoke(message);
            currentNode = currentNode.Previous;
        }
    }

    public static void PostMessage(IEventMessage message)
    {
        PostMessage(message.GetType().GetHashCode(), message);
    }

    public static void PostMessage(int eventId, IEventMessage message)
    {
        PostingList.Add(new PostWrapper
        {
            PostFrame = Engine.GetProcessFrames(),
            EventId = eventId,
            Message = message
        });
    }
}
