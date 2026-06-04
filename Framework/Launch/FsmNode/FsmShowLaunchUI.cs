namespace LccModel
{
    public sealed class FsmShowLaunchUI : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(3);
            LaunchEvent.BroadcastShowVersion(GameConfig.GetVersionStr());
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmRequestVersion>();
        }
    }
}
