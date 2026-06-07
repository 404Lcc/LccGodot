using System.Collections.Generic;
using Godot;

namespace LccHotfix
{
    public class UIRoot : IUIRoot
    {
        private Dictionary<UILayerID, UILayer> _uiLayers = new Dictionary<UILayerID, UILayer>();
        private Dictionary<string, ElementNode> _elementNodes = new Dictionary<string, ElementNode>();

        public UIRoot(Node rootObject)
        {
            _root = rootObject;
        }

        private Node _root;
        private Node _transform;
        private Control _canvas;

        public Control Canvas => _canvas ??= Transform.GetNode<Control>("Canvas");
        public Node Transform => _transform ??= _root;

        public void Initialize()
        {
            _root ??= CreateRootObject();
            _root.Name = "UIRoot";
            _root.SetParent(null);

            var canvasTransform = Canvas;
            for (UILayerID layerId = UILayerID.HUD; layerId <= UILayerID.Debug; layerId++)
            {
                var layer = new UILayer(this, layerId);
                layer.Create(canvasTransform);
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

            _transform = null;
            _canvas = null;

            _root.SetParent(null);
            _root.QueueFree();
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
            var root = new Node() { Name = "UIRoot" };

            // 创建UI画布
            var canvas = new Control() { Name = "Canvas" };

            _canvas = canvas;

            canvas.SetParent(root);

            return root;
        }

        public UILayer GetLayerByID(UILayerID uiLayerId) => _uiLayers[uiLayerId];
    }
}