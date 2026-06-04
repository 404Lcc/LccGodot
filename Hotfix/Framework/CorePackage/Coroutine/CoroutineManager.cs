using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    public static class CoroutineExtension
    {
        public static CoroutineHandler StartCoroutine(this ICoroutine owner, IEnumerator coroutine)
        {
            return Main.CoroutineService!.StartCoroutine(owner, coroutine);
        }

        public static void StopAllCoroutines(this ICoroutine owner)
        {
            Main.CoroutineService!.StopAllCoroutines(owner);
        }
    }

    internal sealed class CoroutineManager : Module, ICoroutineService
    {
        private readonly Dictionary<ICoroutine, List<CoroutineHandler>> _coroutines = new();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            var snapshot = new List<CoroutineHandler>();
            foreach (var list in _coroutines.Values)
            {
                snapshot.AddRange(list);
            }

            foreach (var handler in snapshot)
            {
                handler.Update();
            }
        }

        internal override void Shutdown()
        {
            StopAllTypeCoroutines();
        }

        public CoroutineHandler StartCoroutine(ICoroutine owner, IEnumerator coroutine)
        {
            var handler = new CoroutineHandler(owner, coroutine, Remove);
            if (!_coroutines.TryGetValue(owner, out var list))
            {
                list = new List<CoroutineHandler>();
                _coroutines[owner] = list;
            }

            list.Add(handler);
            return handler;
        }

        public void StopCoroutine(CoroutineHandler handler)
        {
            handler.Stop();
        }

        public void StopAllCoroutines(ICoroutine owner)
        {
            if (!_coroutines.TryGetValue(owner, out var list))
            {
                return;
            }

            foreach (var handler in list.ToArray())
            {
                handler.Stop();
            }

            _coroutines.Remove(owner);
        }

        public void StopAllTypeCoroutines()
        {
            foreach (var list in _coroutines.Values)
            {
                foreach (var handler in list.ToArray())
                {
                    handler.Stop();
                }
            }

            _coroutines.Clear();
        }

        private void Remove(CoroutineHandler handler)
        {
            if (!_coroutines.TryGetValue(handler.Owner, out var list))
            {
                return;
            }

            list.Remove(handler);
            if (list.Count == 0)
            {
                _coroutines.Remove(handler.Owner);
            }
        }
    }
}
