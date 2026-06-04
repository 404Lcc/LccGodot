using Godot;
using LccGodot.Core;
using LccGodot.Events;

namespace LccGodot.Launch;

public partial class Launcher : Node
{
	private bool _started;

	public override async void _Ready()
	{
		GD.Print("Launcher start");
		await Main.SetMainAsync(new GameMain());
		_started = true;
	}

	public override void _Process(double delta)
	{
		if (!_started || Main.Current == null)
		{
			return;
		}

		Main.Current.Update(delta, delta);
		Main.Current.LateUpdate();
		Event.Update();
	}

	public override void _ExitTree()
	{
		if (Main.Current == null)
		{
			return;
		}

		Main.Current.Shutdown();
		GD.Print("Launcher shutdown");
	}
}
