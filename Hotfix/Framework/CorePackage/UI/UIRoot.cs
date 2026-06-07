using System.Collections.Generic;
using Godot;

namespace LccHotfix
{
    public class UIConstant
    {
        public const int LayerMaskUI = 5;
    }

    public class UIRoot : IUIRoot
    {
        private readonly Dictionary<UILayerID, UILayer> _uiLayers = new Dictionary<UILayerID, UILayer>();
        private readonly Dictionary<string, ElementNode> _elementNodes = new Dictionary<string, ElementNode>();

        private Node _root;
        private Control _canvas;
        private Camera2D _uiCamera;

        public UIRoot(Node rootObject = null)
        {
            _root = rootObject;
        }

        public Camera2D RenderCamera => _uiCamera ??= Transform.GetNodeOrNull<Camera2D>("UICamera");
        public Control Canvas => _canvas ??= Transform.GetNodeOrNull<Control>("Canvas");
        public Node Transform => _root;

        public void Initialize()
        {
            _root ??= CreateRootObject();
            _root.Name = "UIRoot";

            if (_root.GetParent() == null && Engine.GetMainLoop() is SceneTree tree)
            {
                tree.Root.AddChild(_root);
            }

            for (UILayerID layerId = UILayerID.HUD; layerId <= UILayerID.Debug; layerId++)
            {
                var layer = new UILayer(this, layerId);
                layer.Create(Canvas);
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
            _uiCamera = null;

            _root?.QueueFree();
            _root = null;
        }

        public ElementNode Find(string name)
        {
            return _elementNodes.TryGetValue(name, out var node) ? node : null;
        }

        public bool Find(ElementNode elementNode, out string name)
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
            if (elementNode is null)
                return;

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

        private Node CreateRootObject()
        {
            var root = new Node { Name = "UIRoot" };

            _uiCamera = new Camera2D { Name = "UICamera" };
            root.AddChild(_uiCamera);

            _canvas = new Control { Name = "Canvas" };
            UILayer.AttachToParent(_canvas, null);
            root.AddChild(_canvas);

            return root;
        }

        public UILayer GetLayerByID(UILayerID uiLayerId) => _uiLayers[uiLayerId];
    }
}
