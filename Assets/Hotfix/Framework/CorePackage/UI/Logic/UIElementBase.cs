using System;
using Godot;
using LccGodot.Services.UI.Node;

namespace LccGodot.Services.UI.Logic;

public abstract class UIElementBase : IUIElementLogic
{
    public UINode Node { get; set; } = null!;

    public Godot.Node? ViewNode => (Node as ElementNode)?.ViewNode;

    public Control? Control => ViewNode as Control;

    public EscapeType EscapeType { get; protected set; } = EscapeType.Hide;

    public ReleaseType ReleaseType { get; protected set; } = ReleaseType.Auto;

    public UILayerId LayerId { get; protected set; } = UILayerId.Panel;

    public bool IsFullScreen { get; protected set; } = true;

    public NodeType ReturnNodeType { get; protected set; } = NodeType.Element;

    public string ReturnNodeName { get; protected set; } = string.Empty;

    public object[]? ReturnNodeArgs { get; protected set; }

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

    protected object? Hide()
    {
        return Node.Hide();
    }
}
