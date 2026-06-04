using Godot;
using LccGodot.Services.UI;
using LccGodot.Services.UI.Logic;

namespace LccGodot.GameLogic.UI;

public sealed class UIMainPanel : UIElementBase
{
    public override void OnConstruct()
    {
        LayerId = UILayerId.Panel;
        IsFullScreen = true;
        EscapeType = EscapeType.Hide;
        ReleaseType = ReleaseType.Keep;
    }

    public override void OnShow(object[] args)
    {
        GD.Print("UI show: UIMainPanel");
    }

    public override object? OnHide()
    {
        GD.Print("UI hide: UIMainPanel");
        return null;
    }
}
