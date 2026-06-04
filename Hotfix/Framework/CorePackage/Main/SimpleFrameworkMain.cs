namespace LccHotfix
{
    public sealed class SimpleFrameworkMain : Main
    {
        public override void OnInstall()
        {
            Log.SetLogHelper(new DefaultLogHelper());

            CodeTypesService = AddModule<CodeTypesManager>();
            CoroutineService = AddModule<CoroutineManager>();
            ValueEventService = AddModule<ValueEventManager>();
            ThreadSyncService = AddModule<ThreadSyncManager>();
            TimerService = AddModule<TimerManager>();

            AssetService = AddModule<AssetManager>();
            UIService = AddModule<UIManager>();
        }
    }
}
