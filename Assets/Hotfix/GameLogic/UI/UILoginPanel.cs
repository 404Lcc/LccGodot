using Godot;
using LccGodot.Services.UI;
using LccGodot.Services.UI.Logic;
using LccGodot.Services.Procedure;

namespace LccGodot.GameLogic.UI;

public sealed class UILoginPanel : UIElementBase
{
    private bool _startButtonBound;

    public override void OnConstruct()
    {
        LayerId = UILayerId.Panel;
        IsFullScreen = true;
        EscapeType = EscapeType.Skip;
        ReleaseType = ReleaseType.Keep;
    }

    public override void OnShow(object[] args)
    {
        GD.Print("UI show: UILoginPanel");
        if (!_startButtonBound && ViewNode?.GetNodeOrNull<Button>("Panel/StartButton") is { } button)
        {
            button.Pressed += OnStartPressed;
            _startButtonBound = true;
        }
    }

    public override object? OnHide()
    {
        GD.Print("UI hide: UILoginPanel");
        return null;
    }

    private static void OnStartPressed()
    {
        Core.Main.ProcedureService.ChangeProcedure(ProcedureType.Main.ToInt());
    }
}
