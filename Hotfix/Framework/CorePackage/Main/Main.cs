using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public abstract partial class Main : Module, IMainService
    {
        private readonly LinkedList<Module> _modules = new();
        private readonly object _lock = new();

        public static Main? Current { get; private set; }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (_lock)
            {
                foreach (var module in _modules)
                {
                    module.Update(elapseSeconds, realElapseSeconds);
                }
            }
        }

        internal override void LateUpdate()
        {
            lock (_lock)
            {
                foreach (var module in _modules)
                {
                    module.LateUpdate();
                }
            }
        }

        internal override void Shutdown()
        {
            lock (_lock)
            {
                for (var node = _modules.Last; node != null; node = node.Previous)
                {
                    node.Value.Shutdown();
                }

                _modules.Clear();
                Current = null;
            }
        }

        public abstract void OnInstall();

        public virtual void OnInitialize()
        {
        }

        public T AddModule<T>() where T : Module, IService, new()
        {
            lock (_lock)
            {
                var module = new T();
                _modules.AddLast(module);
                return module;
            }
        }

        public static void SetMain(Main main)
        {
            if (Current != null)
            {
                throw new InvalidOperationException("Main already exists.");
            }

            Current = main;
            main.OnInstall();
            main.OnInitialize();
        }

        public static void Tick(float elapseSeconds, float realElapseSeconds)
        {
            Current?.Update(elapseSeconds, realElapseSeconds);
        }

        public static void LateTick()
        {
            Current?.LateUpdate();
        }

        public static void Close()
        {
            Current?.Shutdown();
        }
    }
}
