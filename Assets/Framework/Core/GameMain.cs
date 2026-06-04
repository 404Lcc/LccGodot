using System.Threading.Tasks;
using Godot;
using LccGodot.Services.Asset;
using LccGodot.Services.Procedure;
using LccGodot.Services.UI;
using LccGodot.Services.UI.Root;

namespace LccGodot.Core;

public sealed class GameMain : Main
{
    public override void OnInstall()
    {
        GD.Print("Main install");

        AssetService = AddModule<AssetService>();
        UIService = AddModule<UIManager>();
        ProcedureService = AddModule<ProcedureManager>();
    }

    public override Task OnInitializeAsync()
    {
        GD.Print("Main initialize");
        UIService.Init(new UIRoot());
        ProcedureService.ChangeProcedure(ProcedureType.Login.ToInt());
        return Task.CompletedTask;
    }
}
