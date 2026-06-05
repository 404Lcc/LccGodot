namespace LccHotfix
{
    public sealed class SimpleFrameworkMain : Main
    {
        public override void OnInstall()
        {
            Log.SetLogHelper(new DefaultLogHelper());

            CodeTypesService = AddModule<CodeTypesManager>();
            ValueEventService = AddModule<ValueEventManager>();
            ThreadSyncService = AddModule<ThreadSyncManager>();

            AssetService = AddModule<AssetManager>();
            UIService = AddModule<UIManager>();
        }
    }
}
