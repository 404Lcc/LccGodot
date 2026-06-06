using System.Threading.Tasks;
using Godot;

namespace LccHotfix
{
    public interface IAssetService : IService
    {
        public ResourcePackage DefaultPackage { get; }

        public ResourcePackage RawFilePackage { get; }

        public void LoadAssetAsync(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0);

        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0) where T : Resource;

        public void LoadAssetRawFileAsync(string location, System.Action<RawFileHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0);

        public AssetHandle LoadAssetSync(string location, EAssetGroup group = EAssetGroup.Default);

        public AssetHandle LoadAssetSync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : Resource;

        public RawFileHandle LoadAssetRawFileSync(string location, EAssetGroup group = EAssetGroup.Default);

        Task<T> LoadAsync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : Resource;
    }
}
