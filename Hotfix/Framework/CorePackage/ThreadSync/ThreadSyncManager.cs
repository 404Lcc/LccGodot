using System;

namespace LccHotfix
{
    internal sealed class ThreadSyncManager : Module, IThreadSyncService
    {
        private ThreadSynchronizationContext? _context = new();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            _context?.Update();
        }

        internal override void Shutdown()
        {
            _context = null;
        }

        public void Post(Action action)
        {
            _context?.Post(action);
        }
    }
}
