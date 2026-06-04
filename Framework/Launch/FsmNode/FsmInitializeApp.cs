using Godot;

namespace LccModel
{
    public sealed class FsmInitializeApp : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(1);
            EnsureDefaultConfig();
            AssetConfig.PlayMode = GameConfig.IsEnablePatcher ? AssetPlayMode.Host : AssetPlayMode.Offline;
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmStartSplash>();
        }

        private static void EnsureDefaultConfig()
        {
            if (!GameConfig.HasConfig("appVersion"))
            {
                GameConfig.AppVersion = "0";
            }

            if (!GameConfig.HasConfig("localPackageVersion"))
            {
                GameConfig.LocalPackageVersion = "0";
            }

            if (!GameConfig.HasConfig("channel"))
            {
                GameConfig.Channel = 0;
            }

            GD.Print($"[Launch] version {GameConfig.GetVersionStr()}");
        }
    }
}
