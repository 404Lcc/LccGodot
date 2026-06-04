namespace LccHotfix
{
    public abstract partial class Main
    {
        public static ICodeTypesService? CodeTypesService { get; protected set; }
        public static IAssetService? AssetService { get; protected set; }
        public static ICoroutineService? CoroutineService { get; protected set; }
        public static IValueEventService? ValueEventService { get; protected set; }
        public static ITimerService? TimerService { get; protected set; }
        public static IThreadSyncService? ThreadSyncService { get; protected set; }
        public static IUIService? UIService { get; protected set; }
    }
}
