using System.Collections.Generic;

namespace LccHotfix
{
    internal class AssetManager : Module, IAssetService
    {
        public ResourcePackage DefaultPackage { get; private set; }

        private readonly Dictionary<EAssetGroup, AssetLoader> _loader = new();

        public AssetManager()
        {
            DefaultPackage = new ResourcePackage();
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

        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0) where T : Godot.Resource
        {
            GetOrCreateLoader(group).LoadAssetAsync<T>(location, callback, priority);
        }

        public AssetHandle LoadAssetSync(string location, EAssetGroup group = EAssetGroup.Default)
        {
            return GetOrCreateLoader(group).LoadAssetSync(location);
        }

        public AssetHandle LoadAssetSync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : Godot.Resource
        {
            return GetOrCreateLoader(group).LoadAssetSync<T>(location);
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