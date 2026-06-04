namespace LccModel
{
    public sealed class FsmStartGame : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(11);
            LauncherOperation.SetFinish();
        }

        protected override void ChangeToNextState()
        {
        }
    }
}
