using System;
using Godot;

namespace LccHotfix
{
    public sealed class AssetLoader
    {
        public sealed class AssetHandle
        {
            public object? AssetObject { get; init; }
        }

        public void LoadAssetAsync<T>(string asset, Action<AssetHandle> callback) where T : class
        {
            object? result = null;
            if (typeof(T) == typeof(Node))
            {
                result = LoadNode(asset);
            }
            else if (typeof(Resource).IsAssignableFrom(typeof(T)) && ResourceLoader.Exists(asset))
            {
                result = ResourceLoader.Load(asset);
            }

            callback.Invoke(new AssetHandle { AssetObject = result });
        }

        public Node? LoadNode(string asset)
        {
            if (ResourceLoader.Exists(asset))
            {
                var resource = ResourceLoader.Load(asset);
                if (resource is PackedScene scene)
                {
                    return scene.Instantiate();
                }

            }

            return new Control { Name = asset };
        }

        public void Release(string asset)
        {
        }

        public void Release()
        {
        }
    }
}
