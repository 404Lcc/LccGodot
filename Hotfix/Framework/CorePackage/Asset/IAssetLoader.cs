using Godot;

namespace LccHotfix
{
    public interface IAssetLoader
    {
        void Release();
        void Release(string location);
        AssetHandle TryGetAsset(string location);

        void LoadAssetAsync(string location, System.Action<AssetHandle> callback, uint priority = 0);
        void LoadAssetAsync<T>(string location, System.Action<AssetHandle> onCompleted, uint priority = 0) where T : Resource;
        void LoadAssetRawFileAsync(string location, System.Action<RawFileHandle> onCompleted, uint priority = 0);

        AssetHandle LoadAssetSync(string location);
        AssetHandle LoadAssetSync<T>(string location);
        RawFileHandle LoadAssetRawFileSync(string location);
    }
}
