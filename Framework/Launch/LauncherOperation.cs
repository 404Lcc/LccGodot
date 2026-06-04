using Godot;

namespace LccModel
{
    public enum LauncherOperationStatus
    {
        None,
        Running,
        Succeed,
        Failed,
    }

    public sealed class LauncherOperation
    {
        private readonly LaunchStateMachine _machine;

        public LauncherOperationStatus Status { get; private set; } = LauncherOperationStatus.None;
        public string Error { get; private set; } = string.Empty;
        public bool IsDone => Status is LauncherOperationStatus.Succeed or LauncherOperationStatus.Failed;

        public LauncherOperation()
        {
            _machine = new LaunchStateMachine(this);
            _machine.AddNode<FsmInitializeApp>();
            _machine.AddNode<FsmStartSplash>();
            _machine.AddNode<FsmShowLaunchUI>();
            _machine.AddNode<FsmRequestVersion>();
            _machine.AddNode<FsmInitializePackage>();
            _machine.AddNode<FsmRequestPackageVersion>();
            _machine.AddNode<FsmUpdatePackageManifest>();
            _machine.AddNode<FsmCreateDownloader>();
            _machine.AddNode<FsmDownloadPackageFiles>();
            _machine.AddNode<FsmClearCacheBundle>();
            _machine.AddNode<FsmStartGame>();
            _machine.SetBlackboardValue("total", 11);
        }

        public void Start()
        {
            Status = LauncherOperationStatus.Running;
            _machine.Run<FsmInitializeApp>();
        }

        public void Update(double delta)
        {
            if (Status == LauncherOperationStatus.Running)
            {
                _machine.Update(delta);
            }
        }

        public void SetFinish()
        {
            Status = LauncherOperationStatus.Succeed;
        }

        public void SetError(string error)
        {
            Error = error;
            Status = LauncherOperationStatus.Failed;
            GD.PrintErr($"[Launch] {error}");
        }
    }
}
