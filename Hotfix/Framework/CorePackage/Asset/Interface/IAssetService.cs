using Godot;

namespace LccHotfix
{
    public interface IAssetService : IService
    {
        T Load<T>(string path) where T : Resource;
    }
}
