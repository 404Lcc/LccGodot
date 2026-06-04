namespace LccModel
{
    public sealed class FsmRequestVersion : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(4);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmInitializePackage>();
        }
    }
}
