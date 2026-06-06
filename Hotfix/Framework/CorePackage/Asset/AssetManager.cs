using Godot;

namespace LccHotfix
{
    internal sealed class AssetManager : Module, IAssetService
    {
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        internal override void Shutdown()
        {
        }
        
        public T Load<T>(string path) where T : Resource
        {
            return ResourceLoader.Exists(path) ? ResourceLoader.Load<T>(path) : null;
        }
    }
}