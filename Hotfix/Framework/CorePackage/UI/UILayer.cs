using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LccHotfix
{
    public enum UILayerID
    {
        HUD,
        Main,
        Popup,
        Guide,
        Debug
    }

    public class UILayer
    {
        private const int LayerStep = 2048;
        private const int OrderStep = 16;

        private UIRoot _uiRoot;
        private Control _layer;
        private Control _rectTransform;

        public UILayerID UILayerID { get; }
        public List<ElementNode> UIElementList { get; } = new List<ElementNode>();

        public UILayer(UIRoot uiRoot, UILayerID layerID)
        {
            _uiRoot = uiRoot;
            UILayerID = layerID;
        }

        public void Create(Control canvasTransform)
        {
            var go = new Control() { Name = "Layer_" + UILayerID };
            go.MouseFilter = Control.MouseFilterEnum.Ignore;
            AttachToParent(go, canvasTransform);

            _layer = go;
            _rectTransform = go;
        }

        public void Destroy()
        {
            foreach (var item in UIElementList)
            {
                item.GameObject.QueueFree();
            }

            _layer.QueueFree();
            _layer = null;
            _rectTransform = null;
        }

        public void AttachElement(ElementNode elementNode)
        {
            var sortingOrder = 0 == UIElementList.Count ? LayerStep * (int)UILayerID : UIElementList.Last().SortingOrder + OrderStep;
            elementNode.SetSortingOrder(sortingOrder);
            UIElementList.Add(elementNode);
            UIElementList.Sort((l, r) => l.SortingOrder - r.SortingOrder);
        }

        public void AttachElementWidget(ElementNode elementNode)
        {
            var rect = elementNode.RectTransform;
            AttachToParent(rect, _rectTransform);
            rect.PivotOffset = _rectTransform.Size * new Vector2(0.5f, 0.5f);

            var sortingOrder = elementNode.SortingOrder;
            var item = elementNode.GameObject as CanvasItem;
            item.ZIndex += sortingOrder;
        }

        public void DetachElementWidget(ElementNode elementNode)
        {
            var sortingOrder = elementNode.SortingOrder;
            var item = elementNode.GameObject as CanvasItem;
            item.ZIndex -= sortingOrder;
        }

        public void DetachElement(ElementNode elementNode)
        {
            elementNode.SetSortingOrder(0);
            UIElementList.Remove(elementNode);
        }

        public void AttachToParent(Control rect, Control parent)
        {
            if (parent != null)
            {
                if (rect.GetParent() == null)
                {
                    parent.AddChild(rect);
                }
                else if (rect.GetParent() != parent)
                {
                    rect.Reparent(parent);
                }
            }

            rect.Position = Vector2.Zero;
            rect.Rotation = 0;
            rect.Scale = Vector2.One;

            rect.AnchorLeft = 0;
            rect.AnchorTop = 0;

            rect.AnchorRight = 1;
            rect.AnchorBottom = 1;

            rect.OffsetLeft = 0;
            rect.OffsetTop = 0;
            rect.OffsetRight = 0;
            rect.OffsetBottom = 0;
        }
    }
}