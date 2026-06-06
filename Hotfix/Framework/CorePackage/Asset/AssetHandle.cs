using System;
using System.Threading.Tasks;
using Godot;

namespace LccHotfix
{
    public class AssetHandle
    {
        private Action<AssetHandle> _completed;

        public string Location { get; }
        public Resource AssetObject { get; private set; }
        public bool IsDone { get; private set; }

        public event Action<AssetHandle> Completed
        {
            add
            {
                if (IsDone)
                {
                    value?.Invoke(this);
                    return;
                }

                _completed += value;
            }
            remove => _completed -= value;
        }

        public AssetHandle(string location)
        {
            Location = location;
        }

        public void SetResult(Resource asset)
        {
            if (IsDone)
            {
                return;
            }

            AssetObject = asset;
            IsDone = true;
            _completed?.Invoke(this);
            _completed = null;
        }

        public void Release()
        {
            AssetObject = null;
            _completed = null;
            IsDone = true;
        }
    }

    public class RawFileHandle
    {
        private Action<RawFileHandle> _completed;

        public string Location { get; }
        public byte[] Data { get; private set; }
        public string Text { get; private set; }
        public bool IsDone { get; private set; }

        public event Action<RawFileHandle> Completed
        {
            add
            {
                if (IsDone)
                {
                    value?.Invoke(this);
                    return;
                }

                _completed += value;
            }
            remove => _completed -= value;
        }

        public RawFileHandle(string location)
        {
            Location = location;
        }

        public void SetResult(byte[] data)
        {
            Data = data;
            Text = data != null ? System.Text.Encoding.UTF8.GetString(data) : null;
            IsDone = true;
            _completed?.Invoke(this);
            _completed = null;
        }

        public void Release()
        {
            Data = null;
            Text = null;
            _completed = null;
            IsDone = true;
        }
    }

    public class ResourcePackage
    {
        public AssetHandle LoadAssetAsync(string location, uint priority = 0)
        {
            var handle = new AssetHandle(location);
            _ = LoadAssetAsyncInternal(location, handle);
            return handle;
        }

        public AssetHandle LoadAssetAsync(string location, Type type, uint priority = 0)
        {
            return LoadAssetAsync(location, priority);
        }

        public AssetHandle LoadAssetSync(string location)
        {
            var handle = new AssetHandle(location);
            handle.SetResult(ResourceLoader.Load(location));
            return handle;
        }

        public AssetHandle LoadAssetSync(string location, Type type)
        {
            return LoadAssetSync(location);
        }

        public RawFileHandle LoadRawFileAsync(string location, uint priority = 0)
        {
            var handle = new RawFileHandle(location);
            _ = LoadRawFileAsyncInternal(location, handle);
            return handle;
        }

        public RawFileHandle LoadRawFileSync(string location)
        {
            var handle = new RawFileHandle(location);
            handle.SetResult(FileAccess.GetFileAsBytes(location));
            return handle;
        }

        private static async Task LoadAssetAsyncInternal(string location, AssetHandle handle)
        {
            if (!ResourceLoader.Exists(location))
            {
                handle.SetResult(null);
                return;
            }

            var error = ResourceLoader.LoadThreadedRequest(location);
            if (error != Error.Ok)
            {
                handle.SetResult(null);
                return;
            }

            while (ResourceLoader.LoadThreadedGetStatus(location) == ResourceLoader.ThreadLoadStatus.InProgress)
            {
                await Task.Delay(1);
            }

            handle.SetResult(ResourceLoader.LoadThreadedGetStatus(location) == ResourceLoader.ThreadLoadStatus.Loaded
                ? ResourceLoader.LoadThreadedGet(location)
                : null);
        }

        private static async Task LoadRawFileAsyncInternal(string location, RawFileHandle handle)
        {
            await Task.Yield();
            handle.SetResult(FileAccess.GetFileAsBytes(location));
        }
    }
}
