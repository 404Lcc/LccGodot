using System.Collections.Generic;
using Godot;
using LccGodot.Services.UI.Node;

namespace LccGodot.Services.UI.Root;

public sealed class UIRoot : IUIRoot
{
    private readonly Dictionary<UILayerId, UILayer> _layers = new();
    private readonly Dictionary<string, ElementNode> _elements = new();
    private CanvasLayer? _canvasLayer;
    private Control? _rootControl;

    public void Initialize()
    {
        if (_canvasLayer != null)
        {
            return;
        }

        SceneTree tree = (SceneTree)Engine.GetMainLoop();
        _canvasLayer = new CanvasLayer { Name = "UIRoot" };
        _rootControl = new Control
        {
            Name = "Canvas",
            LayoutMode = 1,
            AnchorsPreset = (int)Control.LayoutPreset.FullRect,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _rootControl.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        _canvasLayer.AddChild(_rootControl);

        for (UILayerId layerId = UILayerId.Hud; layerId <= UILayerId.Debug; layerId++)
        {
            UILayer layer = new(layerId);
            _layers[layerId] = layer;
            _rootControl.AddChild(layer.Root);
        }

        tree.Root.CallDeferred(Godot.Node.MethodName.AddChild, _canvasLayer);
    }

    public void FinalizeRoot()
    {
        _layers.Clear();
        _elements.Clear();
        _canvasLayer?.QueueFree();
        _canvasLayer = null;
        _rootControl = null;
    }

    public ElementNode? Find(string name)
    {
        return _elements.GetValueOrDefault(name);
    }

    public void Attach(string name, ElementNode elementNode)
    {
        if (_elements.ContainsKey(name))
        {
            return;
        }

        _elements[name] = elementNode;
        elementNode.AttachToRoot(this);
    }

    public void Detach(ElementNode elementNode)
    {
        string? removeKey = null;
        foreach ((string key, ElementNode value) in _elements)
        {
            if (ReferenceEquals(value, elementNode))
            {
                removeKey = key;
                break;
            }
        }

        if (removeKey != null)
        {
            _elements.Remove(removeKey);
        }

        elementNode.DetachFromRoot();
    }

    public UILayer GetLayerById(UILayerId layerId)
    {
        return _layers[layerId];
    }
}
