namespace LccModel
{
    public sealed class FsmRequestPackageVersion : FsmLaunchStateNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            BroadcastShowProgress(6);
            ChangeToNextState();
        }

        protected override void ChangeToNextState()
        {
            Machine.ChangeState<FsmUpdatePackageManifest>();
        }
    }
}
