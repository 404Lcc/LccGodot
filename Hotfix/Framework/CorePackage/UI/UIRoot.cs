using System.Collections.Generic;
using Godot;

namespace LccHotfix
{
    public static class UIConstant
    {
        public const int LayerMaskUI = 5;
    }

    public sealed class UIRoot : IUIRoot
    {
        private readonly Dictionary<UILayerID, UILayer> _uiLayers = new();
        private readonly Dictionary<string, ElementNode> _elementNodes = new();
        private Node? _root;
        private Control? _canvas;

        public UIRoot(Node? rootObject = null)
        {
            _root = rootObject;
        }

        public Node? RenderCamera => null;
        public Control Canvas => _canvas ??= CreateCanvas();
        public Node Transform => _root ??= CreateRootObject();

        public void Initialize()
        {
            _root ??= CreateRootObject();
            _root.Name = "UIRoot";

            var canvas = Canvas;
            if (canvas.GetParent() == null)
            {
                _root.AddChild(canvas);
            }

            for (var layerId = UILayerID.HUD; layerId <= UILayerID.Debug; layerId++)
            {
                var layer = new UILayer(this, layerId);
                layer.Create(canvas);
                _uiLayers[layerId] = layer;
            }
        }

        public void Finalize()
        {
            foreach (var layerInfo in _uiLayers)
            {
                layerInfo.Value.Destroy();
            }

            _uiLayers.Clear();
            _elementNodes.Clear();
            _canvas = null;
            _root?.QueueFree();
            _root = null;
        }

        public ElementNode? Find(string name)
        {
            return _elementNodes.TryGetValue(name, out var node) ? node : null;
        }

        public bool Find(ElementNode elementNode, out string? name)
        {
            foreach (var kv in _elementNodes)
            {
                if (kv.Value.Equals(elementNode))
                {
                    name = kv.Key;
                    return true;
                }
            }

            name = null;
            return false;
        }

        public void Attach(string name, ElementNode elementNode)
        {
            if (_elementNodes.ContainsKey(name))
            {
                return;
            }

            _elementNodes[name] = elementNode;
            elementNode.AttachedToRoot(this);
        }

        public void Detach(ElementNode elementNode)
        {
            if (elementNode == null)
            {
                return;
            }

            foreach (var kv in _elementNodes)
            {
                if (kv.Value.Equals(elementNode))
                {
                    _elementNodes.Remove(kv.Key);
                    break;
                }
            }

            elementNode.DetachedFromRoot();
        }

        public UILayer GetLayerByID(UILayerID layerID)
        {
            return _uiLayers[layerID];
        }

        private Node CreateRootObject()
        {
            return new Node { Name = "UIRoot" };
        }

        private Control CreateCanvas()
        {
            return new Control
            {
                Name = "Canvas",
                AnchorRight = 1,
                AnchorBottom = 1,
            };
        }
    }
}
