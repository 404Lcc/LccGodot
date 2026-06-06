using Godot;
using System;
using LccHotfix;

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
		public string Error { get; private set; } = string.Empty;

		public void Start()
		{
			Status = LauncherOperationStatus.Running;

			Init.Start();
			SetFinish();
		}

		private void SetFinish()
		{
			Status = LauncherOperationStatus.Succeed;
		}
	}
}
