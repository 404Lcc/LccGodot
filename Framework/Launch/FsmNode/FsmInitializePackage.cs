namespace LccModel
{
    public sealed class FsmInitializePackage : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(5);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmRequestPackageVersion>();
        }
    }
}
