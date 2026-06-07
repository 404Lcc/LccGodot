using System.Collections.Generic;
using System.Linq;
using Godot;
using LccHotfix;

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

    private readonly UIRoot _uiRoot;
    private Control _layer;

    public UILayerID UILayerID { get; }
    public List<ElementNode> UIElementList { get; } = new List<ElementNode>();

    public UILayer(UIRoot uiRoot, UILayerID layerID)
    {
        _uiRoot = uiRoot;
        UILayerID = layerID;
    }

    public void Create(Control canvasTransform)
    {
        _layer = new Control { Name = "Layer_" + UILayerID };
        AttachToParent(_layer, canvasTransform);
    }

    public void Destroy()
    {
        foreach (var item in UIElementList)
        {
            item.GameObject?.QueueFree();
        }

        _layer?.QueueFree();
        _layer = null;
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
        AttachToParent(elementNode.RectTransform, _layer);
        elementNode.RectTransform.PivotOffset = elementNode.RectTransform.Size * 0.5f;
        SetCanvasItemZ(elementNode.GameObject, elementNode.SortingOrder);
    }

    public void DetachElementWidget(ElementNode elementNode)
    {
        SetCanvasItemZ(elementNode.GameObject, 0);
    }

    public void DetachElement(ElementNode elementNode)
    {
        elementNode.SetSortingOrder(0);
        UIElementList.Remove(elementNode);
    }

    public static void AttachToParent(Control rect, Control parent)
    {
        if (rect == null)
        {
            return;
        }

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

        rect.AnchorLeft = 0;
        rect.AnchorTop = 0;
        rect.AnchorRight = 1;
        rect.AnchorBottom = 1;
        rect.OffsetLeft = 0;
        rect.OffsetTop = 0;
        rect.OffsetRight = 0;
        rect.OffsetBottom = 0;
        rect.Scale = Vector2.One;
        rect.Rotation = 0;
        rect.Position = Vector2.Zero;
    }

    private static void SetCanvasItemZ(Node node, int zIndex)
    {
        if (node is CanvasItem canvasItem)
        {
            canvasItem.ZIndex = zIndex;
        }
    }
}
