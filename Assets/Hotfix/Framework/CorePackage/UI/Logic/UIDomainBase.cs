using System;
using LccGodot.Services.UI.Node;

namespace LccGodot.Services.UI.Logic;

public abstract class UIDomainBase : IUIDomainLogic
{
    public UINode Node { get; set; } = null!;

    public EscapeType EscapeType { get; protected set; } = EscapeType.Hide;

    public ReleaseType ReleaseType { get; protected set; } = ReleaseType.Auto;

    public virtual void OnConstruct()
    {
    }

    public virtual void OnCreate()
    {
    }

    public virtual void OnSwitch(Action<bool> callback)
    {
        callback.Invoke(true);
    }

    public virtual void OnCovered(bool covered)
    {
    }

    public virtual void OnShow(object[] args)
    {
    }

    public virtual void OnReShow(object[] args)
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual object? OnHide()
    {
        return null;
    }

    public virtual void OnDestroy()
    {
    }

    public virtual bool OnEscape(ref EscapeType escapeType)
    {
        escapeType = Node.EscapeType;
        return escapeType != EscapeType.Skip;
    }

    public virtual void OnAddChildNode(ElementNode node)
    {
    }

    public virtual void OnRemoveChildNode(ElementNode node)
    {
    }

    public virtual bool OnRequireEscape(ElementNode node)
    {
        return true;
    }

    protected object? Hide()
    {
        return Node.Hide();
    }
}
