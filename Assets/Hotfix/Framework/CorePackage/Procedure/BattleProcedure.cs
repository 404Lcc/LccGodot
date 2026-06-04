using Godot;

namespace LccGodot.Services.Procedure;

[Procedure]
public sealed class BattleProcedure : LoadProcedureHandler
{
    public BattleProcedure()
    {
        ProcedureType = global::LccGodot.Services.Procedure.ProcedureType.Battle.ToInt();
    }

    public override void ProcedureStartHandler()
    {
        base.ProcedureStartHandler();
        GD.Print("Entered BattleProcedure");
    }
}
