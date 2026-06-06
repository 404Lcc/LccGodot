using Godot;
using System;

namespace LccModel
{
    public enum LauncherOperationStatus
    {
        None,
        Running,
        Succeed,
        Failed,
    }

    public class LauncherOperation
    {
        public LauncherOperationStatus Status { get; private set; } = LauncherOperationStatus.None;

        public void Start()
        {
            Status = LauncherOperationStatus.Running;

            try
            {
                StartGame();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }


        private void StartGame()
        {
            SetFinish();
        }

        private void SetFinish()
        {
            Status = LauncherOperationStatus.Succeed;
        }

        private void SetError(string error)
        {
            Status = LauncherOperationStatus.Failed;
            GD.PrintErr($"[Launch] {error}");
        }
    }
}