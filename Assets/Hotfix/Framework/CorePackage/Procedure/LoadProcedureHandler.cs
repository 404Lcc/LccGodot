using Godot;

namespace LccGodot.Services.Procedure;

public abstract class LoadProcedureHandler
{
    public int ProcedureType { get; protected set; }

    public bool IsLoading { get; internal set; }

    public bool IsCleanup { get; internal set; }

    public virtual bool ProcedureEnterStateHandler()
    {
        return true;
    }

    public virtual void ProcedureLoadHandler()
    {
    }

    public virtual void ProcedureStartHandler()
    {
        IsLoading = false;
        GD.Print($"Procedure start: {GetType().Name}");
    }

    public virtual void ProcedureLoadEndHandler()
    {
        IsLoading = false;
    }

    public virtual void Tick()
    {
    }

    public virtual void LateUpdate()
    {
    }

    public virtual void ProcedureExitHandler()
    {
        GD.Print($"Procedure exit: {GetType().Name}");
    }
}
