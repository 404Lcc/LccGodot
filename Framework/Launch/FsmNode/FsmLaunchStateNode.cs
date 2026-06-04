using Godot;

namespace LccModel
{
    public abstract class FsmLaunchStateNode : ILaunchStateNode
    {
        protected LaunchStateMachine Machine = null!;
        protected LauncherOperation LauncherOperation = null!;

        public virtual void OnCreate(LaunchStateMachine machine)
        {
            Machine = machine;
            LauncherOperation = machine.Owner;
        }

        public virtual void OnEnter()
        {
            GD.Print($"[Launch] OnEnter {GetType().Name}");
            LaunchEvent.BroadcastStateChanged(Machine.PreviousNode, Machine.CurrentNode);
        }

        public virtual void OnUpdate(double delta)
        {
        }

        public virtual void OnExit()
        {
            GD.Print($"[Launch] OnExit {GetType().Name}");
        }

        protected void BroadcastShowProgress(int index)
        {
            var total = Machine.GetBlackboardValue<int>("total");
            if (total <= 0)
            {
                total = 1;
            }

            LaunchEvent.BroadcastShowProgress((float)index / total, $"{index}/{total}");
        }

        protected abstract void ChangeToNextState();
    }
}
