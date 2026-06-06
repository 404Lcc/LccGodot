using System;
using Godot;

public partial class Launcher
{
    private float _elapseSeconds;
    private float _realElapseSeconds;

    public event Action OnFixedUpdate;
    public event Action OnUpdate;
    public event Action OnClose;

    public override void _PhysicsProcess(double delta)
    {
        if (OnFixedUpdate != null)
        {
            OnFixedUpdate();
        }
    }

    public override void _Process(double delta)
    {
        _elapseSeconds = (float)delta;
        _realElapseSeconds = (float)delta;

        if (OnUpdate != null)
        {
            OnUpdate();
        }
    }

    public override void _ExitTree()
    {
        ExcuteClose();
    }

    public void ExcuteClose()
    {
        if (OnClose != null)
        {
            OnClose();
        }
    }
}