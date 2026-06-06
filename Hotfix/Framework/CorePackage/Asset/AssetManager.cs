using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LccHotfix
{
    internal sealed class AssetManager : Module, IAssetService
    {
        private readonly List<IAssetLoadOperation> _loadOperationList = new();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            for (var i = _loadOperationList.Count - 1; i >= 0; i--)
            {
                var operation = _loadOperationList[i];
                operation.Update();

                if (operation.IsDone)
                {
                    _loadOperationList.RemoveAt(i);
                }
            }
        }

        internal override void Shutdown()
        {
            foreach (var operation in _loadOperationList)
            {
                operation.Cancel();
            }

            _loadOperationList.Clear();
        }

        public T Load<T>(string path) where T : Resource
        {
            if (!ResourceLoader.Exists(path))
            {
                return null;
            }

            return ResourceLoader.Load<T>(path);
        }

        public Task<T> LoadAsync<T>(string path) where T : Resource
        {
            if (!ResourceLoader.Exists(path))
            {
                return null;
            }

            var error = ResourceLoader.LoadThreadedRequest(path);
            if (error != Error.Ok)
            {
                return null;
            }

            var operation = new AssetLoadOperation<T>(path);
            _loadOperationList.Add(operation);
            return operation.Task;
        }

        private interface IAssetLoadOperation
        {
            bool IsDone { get; }
            void Update();
            void Cancel();
        }

        private sealed class AssetLoadOperation<T> : IAssetLoadOperation where T : Resource
        {
            private readonly string _path;
            private readonly TaskCompletionSource<T> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public bool IsDone { get; private set; }
            public Task<T> Task => _taskCompletionSource.Task;

            public AssetLoadOperation(string path)
            {
                _path = path;
            }

            public void Update()
            {
                if (IsDone)
                {
                    return;
                }

                var status = ResourceLoader.LoadThreadedGetStatus(_path);
                switch (status)
                {
                    case ResourceLoader.ThreadLoadStatus.InProgress:
                        return;
                    case ResourceLoader.ThreadLoadStatus.Loaded:
                        IsDone = true;
                        _taskCompletionSource.TrySetResult(ResourceLoader.LoadThreadedGet(_path) as T);
                        return;
                    case ResourceLoader.ThreadLoadStatus.Failed:
                    case ResourceLoader.ThreadLoadStatus.InvalidResource:
                    default:
                        IsDone = true;
                        _taskCompletionSource.TrySetResult(null);
                        return;
                }
            }

            public void Cancel()
            {
                IsDone = true;
                _taskCompletionSource.TrySetCanceled();
            }
        }
    }
}