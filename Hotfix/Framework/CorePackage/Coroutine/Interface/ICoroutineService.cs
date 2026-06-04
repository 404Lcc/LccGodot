using System.Collections;

namespace LccHotfix
{
    public interface ICoroutine
    {
    }

    public interface ICoroutineService : IService
    {
        CoroutineHandler StartCoroutine(ICoroutine owner, IEnumerator coroutine);
        void StopCoroutine(CoroutineHandler handler);
        void StopAllCoroutines(ICoroutine owner);
        void StopAllTypeCoroutines();
    }
}
