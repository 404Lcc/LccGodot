namespace LccModel
{
    public sealed class FsmCreateDownloader : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(8);
            Machine.SetBlackboardValue("BV_TotalDownloadCount", 0);
            Machine.SetBlackboardValue("BV_TotalDownloadBytes", 0L);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmDownloadPackageFiles>();
        }
    }
}
