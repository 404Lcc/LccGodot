using System;
using Godot;

public partial class Launcher
{
    private ulong _lastTicksUsec;
    public float deltaTime;
    public float unscaledDeltaTime;

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
        deltaTime = (float)delta;
        unscaledDeltaTime = _lastTicksUsec > 0 ? (nowTicksUsec - _lastTicksUsec) / 1000000f : deltaTime;
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