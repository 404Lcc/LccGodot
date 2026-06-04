namespace LccModel
{
    public sealed class FsmStartSplash : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(2);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmShowLaunchUI>();
        }
    }
}
