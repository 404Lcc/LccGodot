using System;
using LccGodot.Services.UI.Node;

namespace LccGodot.Services.UI.Logic;

public interface IUILogic
{
    UINode Node { get; set; }

    EscapeType EscapeType { get; }

    ReleaseType ReleaseType { get; }

    void OnConstruct();

    void OnCreate();

    void OnSwitch(Action<bool> callback);

    void OnCovered(bool covered);

    void OnShow(object[] args);

    void OnReShow(object[] args);

    void OnUpdate();

    object? OnHide();

    void OnDestroy();

    bool OnEscape(ref EscapeType escapeType);
}
