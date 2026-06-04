using Godot;
using LccGodot.Core;
using LccGodot.GameLogic.UI;

namespace LccGodot.Services.Procedure;

[Procedure]
public sealed class MainProcedure : LoadProcedureHandler
{
    public MainProcedure()
    {
        ProcedureType = global::LccGodot.Services.Procedure.ProcedureType.Main.ToInt();
    }

    public override void ProcedureStartHandler()
    {
        base.ProcedureStartHandler();
        GD.Print("Entered MainProcedure");
        Main.UIService.ShowDomain(UIRootDefine.UIRootMain, UIPanelDefine.UIMainPanel);
    }

    public override void ProcedureExitHandler()
    {
        Main.UIService.HideElement(UIPanelDefine.UIMainPanel);
        base.ProcedureExitHandler();
    }
}
