using System;
using Godot;

public partial class Launcher
{
    private ulong _lastTicksUsec;
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
        var nowTicksUsec = Time.GetTicksUsec();
        var deltaTime = (float)delta;

        _elapseSeconds = deltaTime;
        _realElapseSeconds = _lastTicksUsec > 0 ? (nowTicksUsec - _lastTicksUsec) / 1000000f : deltaTime;
        _lastTicksUsec = nowTicksUsec;

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
        OnClose?.Invoke();
    }
}