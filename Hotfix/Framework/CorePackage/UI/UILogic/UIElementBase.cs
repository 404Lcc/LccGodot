using System;
using Godot;

namespace LccHotfix
{
    public abstract class UIElementBase : IUIElementLogic
    {
        public UINode Node { get; set; } = null!;

        public Node? GameObject
        {
            get
            {
                return Node is ElementNode elementNode ? elementNode.GameObject : null;
            }
        }

        public Control? RectTransform
        {
            get
            {
                return Node is ElementNode elementNode ? elementNode.RectTransform : null;
            }
        }

        public EscapeType EscapeType { get; protected set; }
        public ReleaseType ReleaseType { get; protected set; }
        public UILayerID LayerID { get; protected set; }
        public bool IsFullScreen { get; protected set; }
        public NodeType ReturnNodeType { get; protected set; }
        public string ReturnNodeName { get; protected set; } = string.Empty;
        public int ReturnNodeParam { get; protected set; }

        public virtual void OnConstruct()
        {
        }

        public virtual void OnCreate()
        {
            if (GameObject != null)
            {
                ShowView(GameObject);
            }
        }

        public virtual void OnSwitch(Action<bool> callback)
        {
            callback.Invoke(true);
        }

        public virtual void OnCovered(bool covered)
        {
        }

        public virtual void OnShow(object[] paramsList)
        {
        }

        public virtual void OnReShow(object[] paramsList)
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

        public void ShowView(Node gameObject)
        {
            gameObject.SetMeta("className", GetType().Name);
        }
    }
}
