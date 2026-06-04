using System;
using Godot;

namespace LccHotfix
{
    public sealed class TurnNode
    {
        public string nodeName = string.Empty;
        public NodeType nodeType;
        public object[] nodeParam = Array.Empty<object>();
    }

    public class ElementNode : UINode
    {
        public IUIRoot? UIRoot { get; protected set; }
        public Node? GameObject { get; protected set; }
        public Control? RectTransform { get; protected set; }
        public CanvasLayer? Canvas { get; protected set; }
        public Control? Raycaster { get; protected set; }
        public CanvasItem? CanvasGroup { get; protected set; }
        public TurnNode? ReturnNode { get; protected set; }
        public int SortingOrder { get; protected set; }
        public UILayerID LayerID { get; protected set; }
        public bool IsFullScreen { get; protected set; }
        public NodeType ReturnNodeType { get; protected set; }
        public string ReturnNodeName { get; protected set; } = string.Empty;
        public int ReturnNodeParam { get; protected set; }

        public ElementNode(string nodeName)
        {
            NodeName = nodeName;
            Logic = Main.UIService?.GetUILogic(nodeName, this);
        }

        public override void Covered(bool covered)
        {
            if (IsCovered == covered)
            {
                return;
            }

            IsCovered = covered;
            if (covered)
            {
                Log.Debug($"[UI] 覆盖 {NodeName}");
                DoCovered(covered);
            }
            else
            {
                if (DomainNode != null && DomainNode.IsCovered)
                {
                    return;
                }

                Log.Debug($"[UI] 取消覆盖 {NodeName}");
                DoCovered(covered);
            }
        }

        public override void Show(object[] param)
        {
            if (NodePhase != NodePhase.Create)
            {
                return;
            }

            if (DomainNode != null && DomainNode.NodePhase < NodePhase.Show)
            {
                return;
            }

            Log.Debug($"[UI] 显示 {NodeName}");
            NodePhase = NodePhase.Show;
            DomainNode?.AddChildNode(this);

            if (GameObject is CanvasItem canvasItem)
            {
                canvasItem.Visible = true;
            }

            DoShow(param);
        }

        public override object? Hide()
        {
            if (NodePhase != NodePhase.Show)
            {
                return null;
            }

            Log.Debug($"[UI] 隐藏 {NodeName}");
            DomainNode?.RemoveChildNode(this);
            GetAttachedLayer()?.DetachElementWidget(this);
            UIRoot?.Detach(this);
            ReturnNode = null;
            NodePhase = NodePhase.Create;
            return DoHide();
        }

        public override bool Escape(ref EscapeType escape)
        {
            return DoEscape(ref escape);
        }

        public void AttachedToRoot(IUIRoot uiRoot)
        {
            DoAttachedToRoot(uiRoot);
        }

        public void DetachedFromRoot()
        {
            DoDetachedFromRoot();
        }

        protected override void DoConstruct()
        {
            Logic?.OnConstruct();
            if (Logic is IUIElementLogic logic)
            {
                EscapeType = logic.EscapeType;
                ReleaseType = logic.ReleaseType;
                LayerID = logic.LayerID;
                IsFullScreen = logic.IsFullScreen;
                ReturnNodeType = logic.ReturnNodeType;
                ReturnNodeName = logic.ReturnNodeName;
                ReturnNodeParam = logic.ReturnNodeParam;
            }
        }

        protected override void DoCreate()
        {
            Canvas = new CanvasLayer { Name = $"{NodeName}_Canvas" };
            Raycaster = RectTransform;
            CanvasGroup = GameObject as CanvasItem;
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
            if (GameObject is CanvasItem canvasItem)
            {
                canvasItem.Visible = !covered;
            }

            Logic?.OnCovered(covered);
        }

        protected override void DoShow(object[] param)
        {
            if (!string.IsNullOrEmpty(ReturnNodeName) && ReturnNode == null)
            {
                ReturnNode = new TurnNode
                {
                    nodeName = ReturnNodeName,
                    nodeType = ReturnNodeType,
                };
                if (ReturnNodeParam >= 0)
                {
                    ReturnNode.nodeParam = new object[] { ReturnNodeParam };
                }
            }

            GetAttachedLayer()?.AttachElementWidget(this);
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
            if (GameObject is CanvasItem canvasItem)
            {
                canvasItem.Visible = false;
            }

            var returnValue = Logic?.OnHide();
            Main.UIService?.DispatchNodeHide(NodeName, returnValue);
            Main.UIService?.AddToReleaseQueue(this);
            return returnValue;
        }

        protected override void DoDestroy()
        {
            Logic?.OnDestroy();
            GameObject?.QueueFree();
            GameObject = null;
            RectTransform = null;
            Canvas = null;
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            escape = EscapeType;
            if (escape == EscapeType.Skip)
            {
                return false;
            }

            if (Logic != null && !Logic.OnEscape(ref escape))
            {
                return false;
            }

            return DomainNode == null || DomainNode.RequireEscape(this);
        }

        protected virtual void DoAttachedToRoot(IUIRoot uiRoot)
        {
            UIRoot = uiRoot;
            GetAttachedLayer()?.AttachElement(this);
        }

        protected virtual void DoDetachedFromRoot()
        {
            GetAttachedLayer()?.DetachElement(this);
            UIRoot = null;
        }

        private UILayer? GetAttachedLayer()
        {
            return UIRoot?.GetLayerByID(LayerID);
        }

        public void CreateElement(AssetLoader loader, Action<ElementNode> callback)
        {
            Main.UIService?.LoadAsyncGameObject?.Invoke(loader, NodeName, obj =>
            {
                GameObject = obj ?? new Control();
                GameObject.Name = NodeName;
                RectTransform = GameObject as Control;
                if (RectTransform == null)
                {
                    RectTransform = new Control { Name = $"{NodeName}_Control" };
                    GameObject.AddChild(RectTransform);
                }

                callback.Invoke(this);
            });
        }

        public void SetSortingOrder(int sortingOrder)
        {
            SortingOrder = sortingOrder;
            if (Canvas != null)
            {
                Canvas.Layer = sortingOrder;
            }
        }
    }
}
