using System;
using Godot;
using LccModel;

public partial class Launcher : Node
{
	private bool hasStarted;

	public event Action<double> OnProcessUpdate;
	public event Action<double> OnPhysicsProcessUpdate;
	public event Action OnClose;

	public override void _Ready()
	{
		StartLaunch();
	}

	public override void _Process(double delta)
	{
		OnProcessUpdate?.Invoke(delta);
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
		if (hasStarted)
		{
			return;
		}

		hasStarted = true;

		GD.Print("[Launch] Start");

		var operation = new LauncherOperation();
		operation.Start();

		if (operation.Status == LauncherOperationStatus.Succeed)
		{
			GD.Print("[Launch] launcher succeed");
		}
		else
		{
			GD.PrintErr($"[Launch] launcher error : {operation.Error}");
		}
	}

	public void ExecuteClose()
	{
		OnClose?.Invoke();
	}
}
