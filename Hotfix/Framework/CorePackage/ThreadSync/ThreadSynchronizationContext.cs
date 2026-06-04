using System;
using System.Collections.Concurrent;

namespace LccHotfix
{
    public sealed class ThreadSynchronizationContext
    {
        private readonly ConcurrentQueue<Action> _queue = new();

        public void Post(Action action)
        {
            _queue.Enqueue(action);
        }

        public void Update()
        {
            while (_queue.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }
    }
}
