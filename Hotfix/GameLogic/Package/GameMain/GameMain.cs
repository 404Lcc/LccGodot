using System.Reflection;
using Godot;

namespace LccHotfix
{
    internal class GameMain : Main
    {
        public override void OnInstall()
        {
            Launcher.Instance.OnFixedUpdate += OnFixedUpdate;
            Launcher.Instance.OnUpdate += OnUpdate;
            Launcher.Instance.OnClose += OnClose;

            CodeTypesService = Current.AddModule<CodeTypesManager>();
            CodeTypesService.LoadTypes(new Assembly[] { GetType().Assembly });
            AssetService = Current.AddModule<AssetManager>();
            GameObjectPoolService = Current.AddModule<GameObjectPoolManager>();
            GameObjectPoolService.SetAsyncLoader((location, assetLoader, onComplete) =>
            {
                assetLoader.LoadAssetAsync<PackedScene>(location, (handle) =>
                {
                    var prefab = handle.AssetObject as PackedScene;
                    onComplete(location, prefab);
                });
            });
            ValueEventService = Current.AddModule<ValueEventManager>();
            ThreadSyncService = Current.AddModule<ThreadSyncManager>();
        }

        private static void OnFixedUpdate()
        {
        }

        private static void OnUpdate()
        {
            Main.Current.Update(Launcher.Instance.deltaTime, Launcher.Instance.unscaledDeltaTime);
        }

        private static void OnClose()
        {
            Launcher.Instance.OnFixedUpdate -= OnFixedUpdate;
            Launcher.Instance.OnUpdate -= OnUpdate;
            Launcher.Instance.OnClose -= OnClose;
            Current.Shutdown();
        }
    }

    internal partial class Main
    {
    }
}