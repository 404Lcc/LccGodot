using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace LccHotfix
{
    internal class AssetManager : Module, IAssetService
    {
        public const string DefaultPackageName = "DefaultPackage";
        public const string RawFilePackageName = "RawFilePackage";

        public ResourcePackage DefaultPackage { get; private set; }
        public ResourcePackage RawFilePackage { get; private set; }

        private readonly Dictionary<EAssetGroup, AssetLoader> _loader = new();

        public AssetManager()
        {
            DefaultPackage = new ResourcePackage();
            RawFilePackage = new ResourcePackage();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
            foreach (var loader in _loader.Values)
                loader.Release();
            _loader.Clear();
        }

        public void Release(EAssetGroup group)
        {
            if (_loader.TryGetValue(group, out var loader))
                loader.Release();
            _loader.Remove(group);
        }

        public void Release(string location, EAssetGroup group = EAssetGroup.Default)
        {
            if (_loader.TryGetValue(group, out var loader))
                loader.Release(location);
        }

        public void LoadAssetAsync(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0)
        {
            GetOrCreateLoader(group).LoadAssetAsync(location, callback, priority);
        }

        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0) where T : Resource
        {
            GetOrCreateLoader(group).LoadAssetAsync<T>(location, callback, priority);
        }

        public void LoadAssetRawFileAsync(string location, System.Action<RawFileHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0)
        {
            GetOrCreateLoader(group).LoadAssetRawFileAsync(location, callback, priority);
        }

        public AssetHandle LoadAssetSync(string location, EAssetGroup group = EAssetGroup.Default)
        {
            return GetOrCreateLoader(group).LoadAssetSync(location);
        }

        public AssetHandle LoadAssetSync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : Resource
        {
            return GetOrCreateLoader(group).LoadAssetSync<T>(location);
        }

        public RawFileHandle LoadAssetRawFileSync(string location, EAssetGroup group = EAssetGroup.Default)
        {
            return GetOrCreateLoader(group).LoadAssetRawFileSync(location);
        }

        public Task<T> LoadAsync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : Resource
        {
            var taskCompletionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            LoadAssetAsync<T>(location, handle =>
            {
                taskCompletionSource.TrySetResult(handle.AssetObject as T);
            }, group);
            return taskCompletionSource.Task;
        }

        private AssetLoader GetOrCreateLoader(EAssetGroup group)
        {
            if (_loader.TryGetValue(group, out var loader))
                return loader;
            loader = new AssetLoader();
            _loader.Add(group, loader);
            return loader;
        }
    }
}
