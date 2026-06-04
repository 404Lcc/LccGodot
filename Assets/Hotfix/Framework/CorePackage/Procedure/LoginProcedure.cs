using Godot;
using LccGodot.Core;
using LccGodot.GameLogic.UI;

namespace LccGodot.Services.Procedure;

[Procedure]
public sealed class LoginProcedure : LoadProcedureHandler
{
    public LoginProcedure()
    {
        ProcedureType = global::LccGodot.Services.Procedure.ProcedureType.Login.ToInt();
    }

    public override void ProcedureStartHandler()
    {
        base.ProcedureStartHandler();
        GD.Print("Entered LoginProcedure");
        Main.UIService.ShowDomain(UIRootDefine.UIRootLogin, UIPanelDefine.UILoginPanel);
    }

    public override void ProcedureExitHandler()
    {
        Main.UIService.HideElement(UIPanelDefine.UILoginPanel);
        base.ProcedureExitHandler();
    }
}
