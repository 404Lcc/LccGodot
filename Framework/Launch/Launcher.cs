using System;
using Godot;

namespace LccModel
{
    public partial class Launcher : Node
    {
        private LauncherOperation? _launcherOperation;

        public event Action<double>? OnProcessUpdate;
        public event Action<double>? OnPhysicsProcessUpdate;
        public event Action? OnClose;

        public override void _Ready()
        {
            StartLaunch();
        }

        public override void _Process(double delta)
        {
            OnProcessUpdate?.Invoke(delta);
            _launcherOperation?.Update(delta);

            if (_launcherOperation == null || !_launcherOperation.IsDone)
            {
                return;
            }

            if (_launcherOperation.Status == LauncherOperationStatus.Succeed)
            {
                GD.Print("[Launch] launcher succeed");
            }
            else
            {
                GD.PrintErr($"[Launch] launcher error : {_launcherOperation.Error}");
            }

            _launcherOperation = null;
        }

        public override void _PhysicsProcess(double delta)
        {
            OnPhysicsProcessUpdate?.Invoke(delta);
        }

        public override void _ExitTree()
        {
            ExecuteClose();
        }

        public void StartLaunch()
        {
            if (_launcherOperation != null)
            {
                return;
            }

            GD.Print("[Launch] Start");
            _launcherOperation = new LauncherOperation();
            _launcherOperation.Start();
        }

        public void ExecuteClose()
        {
            OnClose?.Invoke();
        }
    }
}