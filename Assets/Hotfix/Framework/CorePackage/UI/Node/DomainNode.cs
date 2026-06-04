using System;
using System.Collections.Generic;
using Godot;
using LccGodot.Core;
using LccGodot.Services.UI.Logic;

namespace LccGodot.Services.UI.Node;

public sealed class DomainNode : UINode
{
    private readonly List<ElementNode> _children = new();

    public DomainNode(string nodeName)
    {
        NodeName = nodeName;
        Logic = Main.UIService.GetUILogic(nodeName, this);
    }

    public IReadOnlyList<ElementNode> Children => _children;

    public int StackIndex { get; private set; } = -1;

    public void SetStackIndex(int stackIndex)
    {
        StackIndex = stackIndex;
    }

    public override void Covered(bool covered)
    {
        if (IsCovered == covered)
        {
            return;
        }

        IsCovered = covered;
        DoCovered(covered);

        if (covered)
        {
            foreach (ElementNode child in _children)
            {
                child.Covered(true);
            }

            return;
        }

        RefreshChildCoverage();
    }

    public override void Show(object[] args)
    {
        if (NodePhase == NodePhase.Show)
        {
            ReShow(args);
            return;
        }

        NodePhase = NodePhase.Show;
        DoShow(args);
    }

    public override object? Hide()
    {
        if (NodePhase != NodePhase.Show)
        {
            return null;
        }

        Main.UIService.RemoveDomainFromStack(this);

        for (int i = _children.Count - 1; i >= 0; i--)
        {
            ElementNode child = _children[i];
            child.SetDomainNode(null);
            child.Hide();
        }

        _children.Clear();
        StackIndex = -1;
        NodePhase = NodePhase.Create;
        return DoHide();
    }

    public override bool Escape(ref EscapeType escapeType)
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            if (_children[i].Escape(ref escapeType))
            {
                return true;
            }
        }

        return DoEscape(ref escapeType);
    }

    public void AddChildNode(ElementNode node)
    {
        if (_children.Contains(node))
        {
            return;
        }

        _children.Add(node);
        RefreshChildCoverage();
        if (Logic is IUIDomainLogic domainLogic)
        {
            domainLogic.OnAddChildNode(node);
        }
    }

    public void RemoveChildNode(ElementNode node)
    {
        if (!_children.Remove(node))
        {
            return;
        }

        node.SetDomainNode(null);
        TryOpenReturnNode(node);
        RefreshChildCoverage();

        if (Logic is IUIDomainLogic domainLogic)
        {
            domainLogic.OnRemoveChildNode(node);
        }
    }

    public bool RequireEscape(ElementNode node)
    {
        if (Logic is IUIDomainLogic domainLogic && !domainLogic.OnRequireEscape(node))
        {
            return false;
        }

        node.Hide();
        return true;
    }

    public void GetAllChildNode(List<UINode> nodes)
    {
        nodes.Add(this);
        nodes.AddRange(_children);
    }

    public ElementNode? GetTopNode()
    {
        return _children.Count == 0 ? null : _children[^1];
    }

    protected override void DoConstruct()
    {
        Logic.OnConstruct();
        EscapeType = Logic.EscapeType;
        ReleaseType = Logic.ReleaseType;
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
        Logic.OnCovered(covered);
    }

    protected override void DoShow(object[] args)
    {
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
        object? returnValue = Logic.OnHide();
        Main.UIService.DispatchNodeHide(NodeName, returnValue);
        Main.UIService.AddToReleaseQueue(this);
        return returnValue;
    }

    protected override void DoDestroy()
    {
        Logic.OnDestroy();
    }

    protected override bool DoEscape(ref EscapeType escapeType)
    {
        escapeType = EscapeType;
        return escapeType != EscapeType.Skip && Logic.OnEscape(ref escapeType);
    }

    private void RefreshChildCoverage()
    {
        if (IsCovered || _children.Count == 0)
        {
            return;
        }

        int firstVisibleIndex = 0;
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            firstVisibleIndex = i;
            if (_children[i].IsFullScreen)
            {
                break;
            }
        }

        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].Covered(i < firstVisibleIndex);
        }
    }

    private static void TryOpenReturnNode(ElementNode node)
    {
        TurnNode? turn = node.ReturnNode;
        if (turn == null)
        {
            return;
        }

        switch (turn.NodeType)
        {
            case NodeType.Domain:
                Main.UIService.ShowDomain(turn.NodeName, turn.NodeArgs ?? Array.Empty<object>());
                break;
            case NodeType.Element:
                Main.UIService.ShowElement(turn.NodeName, turn.NodeArgs ?? Array.Empty<object>());
                break;
            default:
                GD.PushWarning($"Unknown return node type: {turn.NodeType}");
                break;
        }
    }
}
