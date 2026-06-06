using Godot;
using LccModel;

public partial class Launcher : SingletonNode<Launcher>
{
    public override async void _Ready()
    {
        base._Ready();

        if (Engine.GetMainLoop() is not SceneTree tree)
        {
            return;
        }

        await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
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