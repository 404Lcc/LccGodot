namespace LccModel
{
    public sealed class FsmClearCacheBundle : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(10);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmStartGame>();
        }
    }
}
