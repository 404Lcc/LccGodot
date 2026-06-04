namespace LccModel
{
    public sealed class FsmUpdatePackageManifest : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(7);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmCreateDownloader>();
        }
    }
}
