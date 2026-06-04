using Godot;
using LccGodot.Services.UI.Node;

namespace LccGodot.Services.UI.Root;

public sealed class UILayer
{
    private readonly Control _root;

    public UILayer(UILayerId layerId)
    {
        LayerId = layerId;
        _root = new Control
        {
            Name = layerId.ToString(),
            LayoutMode = 1,
            AnchorsPreset = (int)Control.LayoutPreset.FullRect,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
    }

    public UILayerId LayerId { get; }

    public Control Root => _root;

    public void AttachElement(ElementNode node)
    {
        if (node.ViewNode == null || node.ViewNode.GetParent() == _root)
        {
            return;
        }

        node.ViewNode.GetParent()?.RemoveChild(node.ViewNode);
        _root.AddChild(node.ViewNode);

        if (node.ViewNode is Control control)
        {
            control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        }
    }

    public void DetachElement(ElementNode node)
    {
        if (node.ViewNode?.GetParent() == _root)
        {
            _root.RemoveChild(node.ViewNode);
        }
    }

    public void Destroy()
    {
        if (GodotObject.IsInstanceValid(_root))
        {
            _root.Free();
        }
    }
}
