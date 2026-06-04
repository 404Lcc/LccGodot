using System;
using System.Collections.Generic;
using Godot;

namespace LccHotfix
{
    internal sealed class UIManager : Module, IUIService
    {
        private readonly AssetLoader _assetLoader = new();
        private IUIRoot? _uiRoot;
        private readonly Stack<DomainNode> _domainStack = new();
        private DomainNode? _commonDomain;
        private UINode? _switchingNode;
        private readonly Dictionary<string, Type> _uiLogics = new();
        private readonly List<UINode> _releaseQueue = new();
        private Control? _releaseRoot;
        private readonly int _autoCacheTime = 900;
        private readonly Dictionary<string, Action<object?>> _hideCallback = new();
        private List<UINode> _updateNodes = new();

        public Action<AssetLoader, string, Action<Node?>>? LoadAsyncGameObject { get; set; }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_commonDomain != null)
            {
                _updateNodes.Clear();
                _commonDomain.GetAllChildNode(ref _updateNodes);
                foreach (var node in _updateNodes)
                {
                    node.Update();
                }
            }

            if (_domainStack.Count == 0)
            {
                return;
            }

            var topDomain = _domainStack.Peek();
            _updateNodes.Clear();
            topDomain.GetAllChildNode(ref _updateNodes);
            foreach (var node in _updateNodes)
            {
                node.Update();
            }
        }

        internal override void LateUpdate()
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                HideTopNode();
            }

            UpdateReleaseQueue();
        }

        internal override void Shutdown()
        {
            if (_uiRoot == null)
            {
                return;
            }

            HideAllDomain();
            _commonDomain?.Hide();
            _commonDomain?.Destroy();
            ForceClearReleaseQueue(ReleaseType.Keep);
            _releaseRoot?.QueueFree();
            _releaseRoot = null;
            _uiRoot.Finalize();
            _assetLoader.Release();
            _uiRoot = null;
        }

        public void Init(IUIRoot uiRoot)
        {
            _uiRoot = uiRoot;
            _uiRoot.Initialize();

            LoadAsyncGameObject = (loader, asset, end) =>
            {
                loader.LoadAssetAsync<Node>(asset, handle => end.Invoke(handle.AssetObject as Node));
            };

            foreach (var item in GetType().Assembly.GetTypes())
            {
                if (typeof(IUILogic).IsAssignableFrom(item) && !item.IsAbstract && !item.IsInterface)
                {
                    _uiLogics[item.Name] = item;
                }
            }

            _commonDomain = GetOrCreateDomain("UIDomainCommon");
            _commonDomain.SetStackIndex(0);
            _commonDomain.Show(Array.Empty<object>());
        }

        public IUILogic? GetUILogic(string name, UINode node)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (!_uiLogics.TryGetValue(name, out var monoType))
            {
                return null;
            }

            if (Activator.CreateInstance(monoType) is not IUILogic logic)
            {
                return null;
            }

            logic.Node = node;
            return logic;
        }

        public void ShowDomain(string domainName, string elementName, params object[] args)
        {
            if (string.IsNullOrEmpty(domainName) || string.IsNullOrEmpty(elementName))
            {
                return;
            }

            if (_switchingNode != null)
            {
                Log.Error($"[UI] 切换{_switchingNode.NodeName}节点时，请求显示{domainName}域{elementName}界面");
                return;
            }

            Log.Debug($"[UI] 显示界面{elementName}");
            var domain = GetOrCreateDomain(domainName);

            if (!domain.TryGetChildNode(elementName, out var element) || element == null)
            {
                element = GetOrCreateElement(elementName, out var isNewCreate);
                element.SetDomainNode(domain);
                _uiRoot?.Attach(elementName, element);
                _switchingNode = element;
                if (isNewCreate)
                {
                    element.CreateElement(_assetLoader, node =>
                    {
                        node.Create();
                        SwitchNode(node, args);
                    });
                }
                else
                {
                    SwitchNode(element, args);
                }
            }
            else
            {
                element.SetDomainNode(domain);
                _switchingNode = element;
                SwitchNode(element, args);
            }
        }

        public void ShowDomain(string name, params object[] args)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (_switchingNode != null)
            {
                Log.Error($"[UI] 切换{_switchingNode.NodeName}节点时，请求显示{name}域");
                return;
            }

            Log.Debug($"[UI] 显示域{name}");
            var domain = GetOrCreateDomain(name);
            domain.SetDomainNode(domain);
            _switchingNode = domain;
            SwitchNode(domain, args);
        }

        public void ShowElement(string name, params object[] args)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (_switchingNode != null)
            {
                Log.Error($"[UI] 切换{_switchingNode.NodeName}节点时，请求显示{name}域");
                return;
            }

            Log.Debug($"[UI] 显示界面{name}");
            var domain = GetOrCreateDomain(string.Empty);

            if (!domain.TryGetChildNode(name, out var element) || element == null)
            {
                element = GetOrCreateElement(name, out var isNewCreate);
                element.SetDomainNode(domain);
                _uiRoot?.Attach(name, element);
                _switchingNode = element;
                if (isNewCreate)
                {
                    element.CreateElement(_assetLoader, node =>
                    {
                        node.Create();
                        SwitchNode(node, args);
                    });
                }
                else
                {
                    SwitchNode(element, args);
                }
            }
            else
            {
                element.SetDomainNode(domain);
                _switchingNode = element;
                SwitchNode(element, args);
            }
        }

        public object? HideElement(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (_commonDomain != null && _commonDomain.TryHideChildNode(name, out var commonReturnValue))
            {
                return commonReturnValue;
            }

            if (_domainStack.Count == 0)
            {
                return null;
            }

            Log.Debug($"[UI] 隐藏界面{name}");
            return _domainStack.Peek().TryHideChildNode(name, out var returnValue) ? returnValue : null;
        }

        public void HideTopNode()
        {
            var escape = EscapeType.Hide;
            if (_commonDomain != null && _commonDomain.Escape(ref escape))
            {
                return;
            }

            if (_domainStack.Count == 0)
            {
                return;
            }

            _domainStack.Peek().Escape(ref escape);
        }

        public void HideAllDomain()
        {
            if (_switchingNode != null)
            {
                if (_switchingNode is ElementNode elementNode && elementNode.GameObject == null)
                {
                    _assetLoader.Release(elementNode.NodeName);
                }

                AddToReleaseQueue(_switchingNode);
                _switchingNode = null;
            }

            _commonDomain?.Hide();
            while (_domainStack.Count > 0)
            {
                _domainStack.Pop().Hide();
            }
        }

        public DomainNode? GetDomain(string name)
        {
            if (_domainStack.Count == 0)
            {
                return null;
            }

            foreach (var item in _domainStack)
            {
                if (item.NodeName == name)
                {
                    return item;
                }
            }

            return null;
        }

        public T? GetDomain<T>(string name) where T : UIDomainBase
        {
            return GetDomain(name)?.Logic as T;
        }

        public ElementNode? GetElement(string name)
        {
            var topDomain = GetTopDomain();
            return topDomain != null && topDomain.TryGetChildNode(name, out var node) ? node : null;
        }

        public T? GetElement<T>(string name) where T : UIElementBase
        {
            return GetElement(name)?.Logic as T;
        }

        public DomainNode? GetTopDomain()
        {
            return _domainStack.Count == 0 ? null : _domainStack.Peek();
        }

        public ElementNode? GetTopElement()
        {
            return GetTopDomain()?.GetTopNode() as ElementNode;
        }

        public bool IsElementActive(string name)
        {
            return GetElement(name)?.Active ?? false;
        }

        public void RemoveDomainFromStack(DomainNode node)
        {
            var topDomain = GetTopDomain();
            if (topDomain == null)
            {
                return;
            }

            if (node == topDomain)
            {
                _domainStack.Pop();
                if (_domainStack.Count > 0)
                {
                    topDomain.Covered(false);
                }
                return;
            }

            var list = new List<DomainNode>(_domainStack);
            if (!list.Remove(node))
            {
                return;
            }

            list.Reverse();
            _domainStack.Clear();
            foreach (var item in list)
            {
                item.SetStackIndex(_domainStack.Count);
                _domainStack.Push(item);
            }
        }

        public void AddToReleaseQueue(UINode node)
        {
            if (node == null)
            {
                return;
            }

            node.SetRelease(node.ReleaseType == ReleaseType.Auto ? _autoCacheTime : 0);

            if (_releaseRoot == null && _uiRoot != null)
            {
                _releaseRoot = new Control { Name = "WaitForRelease" };
                _uiRoot.Canvas.AddChild(_releaseRoot);
                _releaseRoot.Position = new Vector2(30000, 0);
            }

            if (node is ElementNode element && element.GameObject != null && _releaseRoot != null)
            {
                if (element.GameObject.GetParent() != null)
                {
                    element.GameObject.GetParent().RemoveChild(element.GameObject);
                }
                _releaseRoot.AddChild(element.GameObject);
            }

            _releaseQueue.Add(node);
        }

        public void ForceClearReleaseQueue(ReleaseType level = ReleaseType.Auto)
        {
            for (var i = _releaseQueue.Count - 1; i >= 0; i--)
            {
                if (_releaseQueue[i].ReleaseType <= level)
                {
                    _releaseQueue[i].Destroy();
                    _releaseQueue.RemoveAt(i);
                }
            }
        }

        public void AddNodeHideCallback(string name, Action<object?> callback)
        {
            if (_hideCallback.TryGetValue(name, out var action))
            {
                action -= callback;
                action += callback;
                _hideCallback[name] = action;
            }
            else
            {
                _hideCallback.Add(name, callback);
            }
        }

        public void RemoveNodeHideCallback(string name, Action<object?> callback)
        {
            if (_hideCallback.TryGetValue(name, out var action))
            {
                action -= callback;
                if (action == null)
                {
                    _hideCallback.Remove(name);
                }
                else
                {
                    _hideCallback[name] = action;
                }
            }
        }

        public void DispatchNodeHide(string name, object? returnValue)
        {
            if (_hideCallback.TryGetValue(name, out var action))
            {
                action.Invoke(returnValue);
            }
        }

        private void UpdateReleaseQueue()
        {
            for (var i = _releaseQueue.Count - 1; i >= 0; i--)
            {
                if (_releaseQueue[i].CanRelease())
                {
                    _releaseQueue[i].Destroy();
                    _releaseQueue.RemoveAt(i);
                }
            }
        }

        private void SwitchNode(UINode node, object[] args)
        {
            node.Switch(canOpen => SwitchEnd(node, canOpen, args));
        }

        private void SwitchEnd(UINode node, bool canOpen, object[] args)
        {
            if (_switchingNode == null || _switchingNode != node)
            {
                return;
            }

            _switchingNode = null;
            if (!canOpen)
            {
                AddToReleaseQueue(node);
                return;
            }

            var domain = node.DomainNode;
            if (domain == null)
            {
                return;
            }

            if (domain != _commonDomain)
            {
                if (domain.StackIndex < 0)
                {
                    if (_domainStack.Count > 0)
                    {
                        _domainStack.Peek().Covered(true);
                    }

                    domain.SetStackIndex(_domainStack.Count);
                    _domainStack.Push(domain);
                    domain.Covered(false);
                    domain.Show(node == domain ? args : Array.Empty<object>());
                }
                else
                {
                    var isTop = _domainStack.Count == node.DomainNode!.StackIndex + 1;
                    if (!isTop)
                    {
                        while (_domainStack.Peek() != domain)
                        {
                            _domainStack.Pop().Hide();
                        }
                    }

                    domain.Covered(false);
                    domain.ReShow(node == domain ? args : Array.Empty<object>());
                }
            }

            if (node is ElementNode element)
            {
                if (domain.ContainsNode(node))
                {
                    if (element.IsFullScreen && domain.NodeList != null)
                    {
                        for (var i = domain.NodeList.Count - 1; i >= 0; i--)
                        {
                            if (domain.NodeList[i] == node)
                            {
                                break;
                            }

                            domain.NodeList[i].Hide();
                        }
                    }

                    node.Covered(false);
                    node.ReShow(args);
                }
                else
                {
                    node.Covered(false);
                    node.Show(args);
                }
            }
        }

        private DomainNode GetOrCreateDomain(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (_domainStack.Count == 0)
                {
                    throw new InvalidOperationException("No active domain.");
                }
                return _domainStack.Peek();
            }

            if (_commonDomain != null && name == _commonDomain.NodeName)
            {
                return _commonDomain;
            }

            foreach (var item in _domainStack)
            {
                if (item.NodeName.Equals(name))
                {
                    return item;
                }
            }

            DomainNode? domain = null;
            for (var i = 0; i < _releaseQueue.Count; i++)
            {
                if (_releaseQueue[i].NodeName.Equals(name))
                {
                    domain = _releaseQueue[i] as DomainNode;
                    _releaseQueue.RemoveAt(i);
                    break;
                }
            }

            if (domain == null)
            {
                domain = new DomainNode(name);
                domain.SetDomainNode(domain);
                domain.Construct();
                domain.Create();
            }

            domain.SetStackIndex(-1);
            return domain;
        }

        private ElementNode GetOrCreateElement(string name, out bool isNewCreate)
        {
            isNewCreate = false;
            for (var i = 0; i < _releaseQueue.Count; i++)
            {
                if (_releaseQueue[i].NodeName.Equals(name))
                {
                    var element = (ElementNode)_releaseQueue[i];
                    _releaseQueue.RemoveAt(i);
                    return element;
                }
            }

            var newElement = new ElementNode(name);
            newElement.Construct();
            isNewCreate = true;
            return newElement;
        }
    }
}
