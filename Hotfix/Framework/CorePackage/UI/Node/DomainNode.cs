using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface IDomainNode
    {
        void AddChildNode(ElementNode node);
        void RemoveChildNode(ElementNode node);
        bool RequireEscape(ElementNode node);
    }

    public class DomainNode : UINode, IDomainNode
    {
        public List<ElementNode>? NodeList { get; protected set; }
        public int StackIndex { get; protected set; }

        public DomainNode(string rootName)
        {
            NodeName = rootName;
            Logic = Main.UIService?.GetUILogic(rootName, this);
        }

        public override void Covered(bool covered)
        {
            if (IsCovered == covered)
            {
                return;
            }

            IsCovered = covered;
            Log.Debug($"[UI] {(covered ? "覆盖" : "取消覆盖")} {NodeName}");
            DoCovered(covered);

            if (NodeList == null || NodeList.Count == 0)
            {
                return;
            }

            if (covered)
            {
                for (var i = NodeList.Count - 1; i >= 0; i--)
                {
                    NodeList[i].Covered(true);
                }
            }
            else
            {
                var fullIndex = NodeList.Count;
                for (var i = NodeList.Count - 1; i >= 0; i--)
                {
                    fullIndex = i;
                    if (NodeList[i].IsFullScreen)
                    {
                        break;
                    }
                }

                if (fullIndex < NodeList.Count)
                {
                    for (var i = fullIndex; i < NodeList.Count; i++)
                    {
                        NodeList[i].Covered(false);
                    }
                }
            }
        }

        public override void Show(object[] param)
        {
            if (NodePhase != NodePhase.Create)
            {
                return;
            }

            Log.Debug($"[UI] 显示 {NodeName}");
            NodePhase = NodePhase.Show;
            DoShow(param);
        }

        public override object? Hide()
        {
            if (NodePhase != NodePhase.Show)
            {
                return null;
            }

            Log.Debug($"[UI] 隐藏 {NodeName}");
            Main.UIService?.RemoveDomainFromStack(this);

            while (NodeList != null && NodeList.Count > 0)
            {
                var child = NodeList[^1];
                NodeList.RemoveAt(NodeList.Count - 1);
                child.SetDomainNode(null);
                child.Hide();
            }

            SetStackIndex(-1);
            NodeList = null;
            NodePhase = NodePhase.Create;
            return DoHide();
        }

        public override bool Escape(ref EscapeType escape)
        {
            if (NodeList != null && NodeList.Count > 0)
            {
                for (var i = NodeList.Count - 1; i >= 0; i--)
                {
                    if (NodeList[i].Escape(ref escape))
                    {
                        return true;
                    }
                }
            }

            return DoEscape(ref escape);
        }

        public void AddChildNode(ElementNode node)
        {
            NodeList ??= new List<ElementNode>();
            NodeList.Add(node);

            if (NodeList.Count > 1)
            {
                var fullIndex = NodeList.Count;
                for (var i = NodeList.Count - 1; i >= 0; i--)
                {
                    fullIndex = i;
                    if (NodeList[i].IsFullScreen)
                    {
                        break;
                    }
                }

                for (var i = 0; i <= NodeList.Count - 2; i++)
                {
                    NodeList[i].Covered(i < fullIndex);
                }
            }

            DoAddChildNode(node);
        }

        public void RemoveChildNode(ElementNode node)
        {
            if (NodeList == null || NodeList.Count == 0)
            {
                return;
            }

            NodeList.Remove(node);
            node.SetDomainNode(null);
            DoRemoveChildNode(node);
        }

        public bool RequireEscape(ElementNode node)
        {
            if (!DoRequireEscape(node))
            {
                return false;
            }

            node.Hide();
            return true;
        }

        protected override void DoConstruct()
        {
            Logic?.OnConstruct();
            if (Logic is IUIDomainLogic logic)
            {
                EscapeType = logic.EscapeType;
                ReleaseType = logic.ReleaseType;
            }
            else
            {
                EscapeType = EscapeType.Hide;
                ReleaseType = ReleaseType.Auto;
            }
        }

        protected override void DoCreate()
        {
            Logic?.OnCreate();
        }

        protected override void DoSwitch(Action<bool> callback)
        {
            if (Logic != null)
            {
                Logic.OnSwitch(callback);
            }
            else
            {
                callback(true);
            }
        }

        protected override void DoCovered(bool covered)
        {
            Logic?.OnCovered(covered);
        }

        protected override void DoShow(object[] param)
        {
            Logic?.OnShow(param);
        }

        protected override void DoReShow(object[] param)
        {
            Logic?.OnReShow(param);
        }

        protected override void DoUpdate()
        {
            Logic?.OnUpdate();
        }

        protected override object? DoHide()
        {
            var returnValue = Logic?.OnHide();
            Main.UIService?.DispatchNodeHide(NodeName, returnValue);
            Main.UIService?.AddToReleaseQueue(this);
            return returnValue;
        }

        protected override void DoDestroy()
        {
            Logic?.OnDestroy();
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            escape = EscapeType;
            if (escape == EscapeType.Skip)
            {
                return false;
            }

            return Logic?.OnEscape(ref escape) ?? true;
        }

        protected void DoAddChildNode(ElementNode node)
        {
            if (Logic is IUIDomainLogic logic)
            {
                logic.OnAddChildNode(node);
            }
        }

        protected void DoRemoveChildNode(ElementNode node)
        {
            if (Active)
            {
                var turn = node.ReturnNode;
                if (turn != null)
                {
                    switch (turn.nodeType)
                    {
                        case NodeType.Domain:
                            Main.UIService?.ShowDomain(turn.nodeName, turn.nodeParam);
                            break;
                        case NodeType.Element:
                            Main.UIService?.ShowElement(turn.nodeName, turn.nodeParam);
                            break;
                    }
                }
            }

            if (Logic is IUIDomainLogic logic)
            {
                logic.OnRemoveChildNode(node);
            }

            if (NodeList == null || NodeList.Count == 0)
            {
                Hide();
                return;
            }

            if (Active && node.IsFullScreen)
            {
                var fullIndex = NodeList.Count;
                for (var i = NodeList.Count - 1; i >= 0; i--)
                {
                    fullIndex = i;
                    if (NodeList[i].IsFullScreen)
                    {
                        break;
                    }
                }

                if (fullIndex < NodeList.Count)
                {
                    for (var i = fullIndex; i < NodeList.Count; i++)
                    {
                        NodeList[i].Covered(false);
                    }
                }
            }
        }

        protected bool DoRequireEscape(ElementNode node)
        {
            return Logic is not IUIDomainLogic logic || logic.OnRequireEscape(node);
        }

        public void SetStackIndex(int index)
        {
            StackIndex = index;
        }

        public bool ContainsNode(UINode node)
        {
            if (NodeList == null)
            {
                return false;
            }

            foreach (var item in NodeList)
            {
                if (item == node)
                {
                    return true;
                }
            }

            return false;
        }

        public void GetAllChildNode(ref List<UINode> list)
        {
            list ??= new List<UINode>();
            list.Add(this);

            if (NodeList == null || NodeList.Count == 0)
            {
                return;
            }

            foreach (var item in NodeList)
            {
                list.Add(item);
            }
        }

        public bool TryGetChildNode(string nodeName, out ElementNode? node)
        {
            node = null;
            if (NodeList == null || NodeList.Count == 0)
            {
                return false;
            }

            foreach (var item in NodeList)
            {
                if (item.NodeName.Equals(nodeName))
                {
                    node = item;
                    return true;
                }
            }

            return false;
        }

        public bool TryHideChildNode(string nodeName, out object? returnValue)
        {
            returnValue = null;
            if (NodeName == nodeName)
            {
                returnValue = Hide();
                return true;
            }

            if (NodeList == null || NodeList.Count == 0)
            {
                return false;
            }

            for (var i = NodeList.Count - 1; i >= 0; i--)
            {
                if (NodeList[i].NodeName == nodeName)
                {
                    returnValue = NodeList[i].Hide();
                    return true;
                }
            }

            return false;
        }

        public UINode GetTopNode()
        {
            return NodeList == null || NodeList.Count == 0 ? this : NodeList[^1];
        }
    }
}
