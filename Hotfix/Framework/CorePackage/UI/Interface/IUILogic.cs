using System;

namespace LccHotfix
{
    public interface IUILogic
    {
        UINode Node { get; set; }
        EscapeType EscapeType { get; }
        ReleaseType ReleaseType { get; }
        void OnConstruct();
        void OnCreate();
        void OnSwitch(Action<bool> callback);
        void OnCovered(bool covered);
        void OnShow(object[] paramsList);
        void OnReShow(object[] paramsList);
        void OnUpdate();
        object? OnHide();
        void OnDestroy();
        bool OnEscape(ref EscapeType escapeType);
    }
}
