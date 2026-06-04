using System;
using LccGodot.Services.UI.Logic;

namespace LccGodot.Services.UI.Node;

public abstract class UINode
{
    private int _releaseTimer;

    public string NodeName { get; protected set; } = string.Empty;

    public bool IsCovered { get; protected set; }

    public NodePhase NodePhase { get; protected set; } = NodePhase.Create;

    public IUILogic Logic { get; protected set; } = null!;

    public DomainNode? DomainNode { get; private set; }

    public bool Active => NodePhase == NodePhase.Show;

    public EscapeType EscapeType { get; protected set; } = EscapeType.Hide;

    public ReleaseType ReleaseType { get; protected set; } = ReleaseType.Auto;

    public void SetDomainNode(DomainNode? domainNode)
    {
        DomainNode = domainNode;
    }

    public void SetRelease(int releaseTimer)
    {
        _releaseTimer = releaseTimer;
    }

    public bool CanRelease()
    {
        if (ReleaseType > ReleaseType.Auto)
        {
            return false;
        }

        _releaseTimer--;
        return _releaseTimer <= 0;
    }

    public void Construct()
    {
        DoConstruct();
    }

    public void Create()
    {
        NodePhase = NodePhase.Create;
        DoCreate();
    }

    public void Switch(Action<bool> callback)
    {
        DoSwitch(callback);
    }

    public abstract void Covered(bool covered);

    public abstract void Show(object[] args);

    public void ReShow(object[] args)
    {
        if (NodePhase == NodePhase.Show)
        {
            DoReShow(args);
        }
    }

    public void Update()
    {
        if (NodePhase == NodePhase.Show)
        {
            DoUpdate();
        }
    }

    public abstract object? Hide();

    public void Destroy()
    {
        DoDestroy();
    }

    public abstract bool Escape(ref EscapeType escapeType);

    protected abstract void DoConstruct();

    protected abstract void DoCreate();

    protected abstract void DoSwitch(Action<bool> callback);

    protected abstract void DoCovered(bool covered);

    protected abstract void DoShow(object[] args);

    protected abstract void DoReShow(object[] args);

    protected abstract void DoUpdate();

    protected abstract object? DoHide();

    protected abstract void DoDestroy();

    protected abstract bool DoEscape(ref EscapeType escapeType);
}
