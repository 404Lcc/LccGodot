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
        Debug,
    }

    public sealed class UILayer
    {
        private const int LayerStep = 2048;
        private const int OrderStep = 16;

        private readonly UIRoot _uiRoot;
        private CanvasLayer? _layer;
        private Control? _rootControl;

        public UILayerID UILayerID { get; }
        public List<ElementNode> UIElementList { get; } = new();

        public UILayer(UIRoot uiRoot, UILayerID layerID)
        {
            _uiRoot = uiRoot;
            UILayerID = layerID;
        }

        public void Create(Node canvasTransform)
        {
            _layer = new CanvasLayer
            {
                Name = "Layer_" + UILayerID,
                Layer = (int)UILayerID,
            };
            _rootControl = new Control
            {
                Name = "Root",
                AnchorRight = 1,
                AnchorBottom = 1,
            };
            _layer.AddChild(_rootControl);
            canvasTransform.AddChild(_layer);
        }

        public void Destroy()
        {
            foreach (var item in UIElementList)
            {
                item.GameObject?.QueueFree();
            }

            UIElementList.Clear();
            _layer?.QueueFree();
            _layer = null;
            _rootControl = null;
        }

        public void AttachElement(ElementNode elementNode)
        {
            var sortingOrder = UIElementList.Count == 0 ? LayerStep * (int)UILayerID : UIElementList.Last().SortingOrder + OrderStep;
            elementNode.SetSortingOrder(sortingOrder);
            UIElementList.Add(elementNode);
            UIElementList.Sort((l, r) => l.SortingOrder - r.SortingOrder);
        }

        public void AttachElementWidget(ElementNode elementNode)
        {
            if (_rootControl == null || elementNode.GameObject == null)
            {
                return;
            }

            AttachToParent(elementNode.GameObject, _rootControl);
            if (elementNode.Canvas != null)
            {
                elementNode.Canvas.Layer = elementNode.SortingOrder;
            }
        }

        public void DetachElementWidget(ElementNode elementNode)
        {
            if (elementNode.GameObject?.GetParent() != null)
            {
                elementNode.GameObject.GetParent().RemoveChild(elementNode.GameObject);
            }
        }

        public void DetachElement(ElementNode elementNode)
        {
            elementNode.SetSortingOrder(0);
            UIElementList.Remove(elementNode);
        }

        public void AttachToParent(Node node, Node parent)
        {
            if (node.GetParent() != null)
            {
                node.GetParent().RemoveChild(node);
            }

            parent.AddChild(node);
            if (node is Control control)
            {
                control.AnchorLeft = 0;
                control.AnchorTop = 0;
                control.AnchorRight = 1;
                control.AnchorBottom = 1;
                control.OffsetLeft = 0;
                control.OffsetTop = 0;
                control.OffsetRight = 0;
                control.OffsetBottom = 0;
            }
        }
    }
}
