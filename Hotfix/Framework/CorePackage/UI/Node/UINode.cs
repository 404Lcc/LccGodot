using System;

namespace LccHotfix
{
    public abstract class UINode
    {
        private int _releaseTimer;

        public string NodeName { get; protected set; } = string.Empty;
        public bool IsCovered { get; protected set; }
        public NodePhase NodePhase { get; protected set; }
        public IUILogic? Logic { get; protected set; }
        public DomainNode? DomainNode { get; protected set; }
        public bool Active => NodePhase == NodePhase.Show;
        public EscapeType EscapeType { get; protected set; }
        public ReleaseType ReleaseType { get; protected set; }

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
        public abstract void Show(object[] param);

        public void ReShow(object[] param)
        {
            if (NodePhase == NodePhase.Show)
            {
                DoReShow(param);
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

        public abstract bool Escape(ref EscapeType escape);
        protected abstract void DoConstruct();
        protected abstract void DoCreate();
        protected abstract void DoSwitch(Action<bool> callback);
        protected abstract void DoCovered(bool covered);
        protected abstract void DoShow(object[] param);
        protected abstract void DoReShow(object[] param);
        protected abstract void DoUpdate();
        protected abstract object? DoHide();
        protected abstract void DoDestroy();
        protected abstract bool DoEscape(ref EscapeType escape);
    }
}
