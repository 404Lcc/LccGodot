using System;
using System.Collections;

namespace LccHotfix
{
    public sealed class CoroutineHandler
    {
        private readonly IEnumerator _coroutine;
        private readonly Action<CoroutineHandler> _onComplete;

        public ICoroutine Owner { get; }
        public bool IsRunning { get; private set; } = true;

        public CoroutineHandler(ICoroutine owner, IEnumerator coroutine, Action<CoroutineHandler> onComplete)
        {
            Owner = owner;
            _coroutine = coroutine;
            _onComplete = onComplete;
        }

        public void Stop()
        {
            IsRunning = false;
            _onComplete.Invoke(this);
        }

        internal void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            if (_coroutine.MoveNext())
            {
                return;
            }

            IsRunning = false;
            _onComplete.Invoke(this);
        }
    }
}
