using System;
using Godot;

public partial class Launcher
{
    private float _elapseSeconds;
    private float _realElapseSeconds;

    public event Action OnFixedUpdate;
    public event Action OnUpdate;
    public event Action OnLateUpdate;
    public event Action OnClose;
    public event Action OnGizmos;

    public override void _PhysicsProcess(double delta)
    {
        if (OnFixedUpdate != null)
        {
            OnFixedUpdate();
        }
    }

    private void Update()
    {
        if (OnUpdate != null)
        {
            OnUpdate();
        }
    }

    private void LateUpdate()
    {
        if (OnLateUpdate != null)
        {
            OnLateUpdate();
        }
    }

    private void OnDrawGizmos()
    {
        if (OnGizmos != null)
        {
            OnGizmos();
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

    public override void _Process(double delta)
    {
        _elapseSeconds = (float)delta;
        _realElapseSeconds = (float)delta;

        Update();
        LateUpdate();
    }
}