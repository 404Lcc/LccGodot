namespace LccHotfix
{
    public interface IAssetService : IService
    {
        public ResourcePackage DefaultPackage { get; }

        public void LoadAssetAsync(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0);

        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0) where T : Godot.Resource;

        public AssetHandle LoadAssetSync(string location, EAssetGroup group = EAssetGroup.Default);

        public AssetHandle LoadAssetSync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : Godot.Resource;
    }
}