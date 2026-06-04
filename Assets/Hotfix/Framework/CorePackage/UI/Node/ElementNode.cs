using System;
using Godot;
using LccGodot.Core;
using LccGodot.Services.UI.Logic;
using LccGodot.Services.UI.Root;

namespace LccGodot.Services.UI.Node;

public sealed class ElementNode : UINode
{
    public ElementNode(string nodeName)
    {
        NodeName = nodeName;
        Logic = Main.UIService.GetUILogic(nodeName, this);
    }

    public IUIRoot? UIRoot { get; private set; }

    public Godot.Node? ViewNode { get; private set; }

    public TurnNode? ReturnNode { get; private set; }

    public UILayerId LayerId { get; private set; } = UILayerId.Panel;

    public bool IsFullScreen { get; private set; }

    public override void Covered(bool covered)
    {
        if (IsCovered == covered)
        {
            return;
        }

        IsCovered = covered;
        DoCovered(covered);
    }

    public override void Show(object[] args)
    {
        if (NodePhase != NodePhase.Create)
        {
            ReShow(args);
            return;
        }

        if (DomainNode != null && DomainNode.NodePhase < NodePhase.Show)
        {
            return;
        }

        NodePhase = NodePhase.Show;
        DomainNode?.AddChildNode(this);
        SetViewVisible(true);
        DoShow(args);
    }

    public override object? Hide()
    {
        if (NodePhase != NodePhase.Show)
        {
            return null;
        }

        DomainNode?.RemoveChildNode(this);
        UIRoot?.Detach(this);
        ReturnNode = null;
        NodePhase = NodePhase.Create;
        return DoHide();
    }

    public override bool Escape(ref EscapeType escapeType)
    {
        return DoEscape(ref escapeType);
    }

    public void AttachToRoot(IUIRoot uiRoot)
    {
        UIRoot = uiRoot;
        uiRoot.GetLayerById(LayerId).AttachElement(this);
    }

    public void DetachFromRoot()
    {
        UIRoot?.GetLayerById(LayerId).DetachElement(this);
        UIRoot = null;
    }

    public void CreateElement(Action<ElementNode> callback)
    {
        Main.UIService.LoadAsyncNode(NodeName, node =>
        {
            ViewNode = node ?? new Control { Name = NodeName };
            ViewNode.Name = NodeName;
            SetViewVisible(false);
            callback.Invoke(this);
        });
    }

    protected override void DoConstruct()
    {
        Logic.OnConstruct();
        if (Logic is IUIElementLogic elementLogic)
        {
            EscapeType = elementLogic.EscapeType;
            ReleaseType = elementLogic.ReleaseType;
            LayerId = elementLogic.LayerId;
            IsFullScreen = elementLogic.IsFullScreen;
        }
    }

    protected override void DoCreate()
    {
        Logic.OnCreate();
    }

    protected override void DoSwitch(Action<bool> callback)
    {
        Logic.OnSwitch(callback);
    }

    protected override void DoCovered(bool covered)
    {
        SetViewVisible(!covered);
        Logic.OnCovered(covered);
    }

    protected override void DoShow(object[] args)
    {
        if (Logic is IUIElementLogic elementLogic && !string.IsNullOrEmpty(elementLogic.ReturnNodeName) && ReturnNode == null)
        {
            ReturnNode = new TurnNode
            {
                NodeName = elementLogic.ReturnNodeName,
                NodeType = elementLogic.ReturnNodeType,
                NodeArgs = elementLogic.ReturnNodeArgs
            };
        }

        Logic.OnShow(args);
    }

    protected override void DoReShow(object[] args)
    {
        Logic.OnReShow(args);
    }

    protected override void DoUpdate()
    {
        Logic.OnUpdate();
    }

    protected override object? DoHide()
    {
        SetViewVisible(false);
        object? returnValue = Logic.OnHide();
        Main.UIService.DispatchNodeHide(NodeName, returnValue);
        Main.UIService.AddToReleaseQueue(this);
        return returnValue;
    }

    protected override void DoDestroy()
    {
        Logic.OnDestroy();
        if (ViewNode != null && GodotObject.IsInstanceValid(ViewNode))
        {
            ViewNode.Free();
        }
        ViewNode = null;
    }

    protected override bool DoEscape(ref EscapeType escapeType)
    {
        escapeType = EscapeType;
        if (escapeType == EscapeType.Skip)
        {
            return false;
        }

        if (!Logic.OnEscape(ref escapeType))
        {
            return false;
        }

        return DomainNode?.RequireEscape(this) ?? true;
    }

    private void SetViewVisible(bool visible)
    {
        switch (ViewNode)
        {
            case CanvasItem canvasItem:
                canvasItem.Visible = visible;
                break;
            case Window window:
                window.Visible = visible;
                break;
        }
    }
}
