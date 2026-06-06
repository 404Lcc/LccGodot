using System.Reflection;

namespace LccHotfix
{
    internal class GameMain : Main
    {
        public override void OnInstall()
        {
            CodeTypesService = Current.AddModule<CodeTypesManager>();
            CodeTypesService.LoadTypes(new Assembly[] { GetType().Assembly });
            AssetService = Current.AddModule<AssetManager>();
            ValueEventService = Current.AddModule<ValueEventManager>();
            ThreadSyncService = Current.AddModule<ThreadSyncManager>();
        }

        private static void OnFixedUpdate()
        {
        }

        private static void OnUpdate()
        {
        }

        private static void OnLateUpdate()
        {
            Main.Current.LateUpdate();
        }

        private static void OnGizmos()
        {
        }

        private static void OnClose()
        {
            Current.Shutdown();
        }
    }

    internal partial class Main
    {
    }
}