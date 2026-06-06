using Godot;
using LccModel;

public partial class Launcher : SingletonNode<Launcher>
{
	public override void _Ready()
	{
		StartLaunch();
	}

	public void StartLaunch()
	{
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
}
