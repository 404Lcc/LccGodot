using Godot;

namespace LccHotfix
{
    internal sealed class AssetManager : Module, IAssetService
    {
        public T? Load<T>(string path) where T : Resource
        {
            return ResourceLoader.Exists(path) ? ResourceLoader.Load<T>(path) : null;
        }
    }
}
