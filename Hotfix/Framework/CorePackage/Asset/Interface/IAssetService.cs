using Godot;
using System.Threading.Tasks;

namespace LccHotfix
{
    public interface IAssetService : IService
    {
        T Load<T>(string path) where T : Resource;
        Task<T> LoadAsync<T>(string path) where T : Resource;
    }
}