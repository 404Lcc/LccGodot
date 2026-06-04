namespace LccModel
{
    public sealed class FsmDownloadPackageFiles : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(9);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmClearCacheBundle>();
        }
    }
}
