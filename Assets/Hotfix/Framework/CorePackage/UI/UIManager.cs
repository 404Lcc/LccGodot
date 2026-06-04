using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using LccGodot.Core;
using LccGodot.Services.UI.Logic;
using LccGodot.Services.UI.Node;
using LccGodot.Services.UI.Root;

namespace LccGodot.Services.UI;

public sealed class UIManager : LccGodot.Core.Module, IUIService
{
    private readonly Stack<DomainNode> _domainStack = new();
    private readonly Dictionary<string, Type> _uiLogics = new();
    private readonly Dictionary<string, ElementNode> _elements = new();
    private readonly Dictionary<string, DomainNode> _domains = new();
    private readonly List<UINode> _releaseQueue = new();
    private readonly List<UINode> _updateNodes = new();
    private readonly Dictionary<string, List<Action<object?>>> _hideCallbacks = new();

    private IUIRoot? _uiRoot;
    private DomainNode? _commonDomain;
    private UINode? _switchingNode;
    private int _autoCacheFrames = 900;

    public Func<string, string> ResolveNodePath { get; set; } = name =>
        name.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || name.StartsWith("user://", StringComparison.OrdinalIgnoreCase)
            ? name
            : $"res://Assets/Res/UI/{name}.tscn";

    internal override void Update(double delta, double realDelta)
    {
        UpdateDomain(_commonDomain);

        if (_domainStack.Count > 0)
        {
            UpdateDomain(_domainStack.Peek());
        }

        UpdateReleaseQueue();
    }

    internal override void Shutdown()
    {
        HideAllDomain();
        _commonDomain?.Hide();
        ForceClearReleaseQueue(ReleaseType.Keep);
        foreach (ElementNode element in _elements.Values.ToArray())
        {
            element.Destroy();
        }

        foreach (DomainNode domain in _domains.Values.ToArray())
        {
            domain.Destroy();
        }

        _uiRoot?.FinalizeRoot();
        _uiRoot = null;
        _commonDomain = null;
        _switchingNode = null;
        _uiLogics.Clear();
        _elements.Clear();
        _domains.Clear();
        _releaseQueue.Clear();
        _hideCallbacks.Clear();
    }

    public void Init(IUIRoot uiRoot)
    {
        _uiRoot = uiRoot;
        _uiRoot.Initialize();
        RegisterUILogics();

        _commonDomain = GetOrCreateDomain("UIDomainCommon");
        _commonDomain.SetStackIndex(0);
        _commonDomain.Show(Array.Empty<object>());
    }

    public void LoadAsyncNode(string name, Action<Godot.Node?> callback)
    {
        string path = ResolveNodePath(name);
        Main.AssetService.LoadAsync<PackedScene>(path, scene =>
        {
            if (scene == null)
            {
                GD.PushWarning($"UI scene not found, using empty Control fallback: {path}");
                callback.Invoke(new Control { Name = name });
                return;
            }

            callback.Invoke(scene.Instantiate());
        });
    }

    public IUILogic GetUILogic(string name, UINode node)
    {
        IUILogic logic = null;
        if (_uiLogics.TryGetValue(name, out Type monoType))
        {
            logic = Activator.CreateInstance(monoType) as IUILogic;
            logic.Node = node;
        }

        return logic;
    }

    public void ShowDomain(string domainName, string elementName, params object[] args)
    {
        if (string.IsNullOrEmpty(domainName) || string.IsNullOrEmpty(elementName))
        {
            return;
        }

        ShowDomain(domainName, args);
        ShowElementInDomain(GetOrCreateDomain(domainName), elementName, args);
    }

    public void ShowDomain(string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name) || _switchingNode != null)
        {
            return;
        }

        DomainNode domain = GetOrCreateDomain(name);
        domain.SetDomainNode(domain);
        _switchingNode = domain;
        SwitchNode(domain, args);
    }

    public void ShowElement(string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name) || _switchingNode != null)
        {
            return;
        }

        DomainNode domain = _domainStack.Count > 0 ? _domainStack.Peek() : GetOrCreateDomain("UIDomainMain");
        if (!domain.Active)
        {
            ShowDomain(domain.NodeName);
        }

        ShowElementInDomain(domain, name, args);
    }

    public object? HideElement(string name)
    {
        return _elements.TryGetValue(name, out ElementNode? node) ? node.Hide() : null;
    }

    public void HideTopNode()
    {
        EscapeType escapeType = EscapeType.Skip;

        ElementNode? commonTop = _commonDomain?.GetTopNode();
        if (commonTop != null && commonTop.Escape(ref escapeType))
        {
            return;
        }

        DomainNode? topDomain = GetTopDomain();
        if (topDomain == null)
        {
            return;
        }

        if (topDomain.Escape(ref escapeType) && escapeType == EscapeType.Hide && topDomain.Children.Count == 0)
        {
            topDomain.Hide();
        }
    }

    public void HideAllDomain()
    {
        while (_domainStack.Count > 0)
        {
            _domainStack.Peek().Hide();
        }
    }

    public DomainNode? GetDomain(string name)
    {
        return _domains.GetValueOrDefault(name);
    }

    public T? GetDomain<T>(string name) where T : UIDomainBase
    {
        return GetDomain(name)?.Logic as T;
    }

    public ElementNode? GetElement(string name)
    {
        return _elements.GetValueOrDefault(name);
    }

    public T? GetElement<T>(string name) where T : UIElementBase
    {
        return GetElement(name)?.Logic as T;
    }

    public DomainNode? GetTopDomain()
    {
        return _domainStack.Count > 0 ? _domainStack.Peek() : null;
    }

    public ElementNode? GetTopElement()
    {
        return GetTopDomain()?.GetTopNode();
    }

    public bool IsElementActive(string name)
    {
        return _elements.TryGetValue(name, out ElementNode? node) && node.Active;
    }

    public void RemoveDomainFromStack(DomainNode node)
    {
        if (!_domainStack.Contains(node))
        {
            return;
        }

        Stack<DomainNode> temp = new();
        while (_domainStack.Count > 0)
        {
            DomainNode current = _domainStack.Pop();
            if (!ReferenceEquals(current, node))
            {
                temp.Push(current);
            }
        }

        while (temp.Count > 0)
        {
            _domainStack.Push(temp.Pop());
        }

        RefreshDomainCoverage();
    }

    public void AddToReleaseQueue(UINode node)
    {
        if (node.ReleaseType == ReleaseType.Keep || _releaseQueue.Contains(node))
        {
            return;
        }

        node.SetRelease(_autoCacheFrames);
        _releaseQueue.Add(node);
    }

    public void ForceClearReleaseQueue(ReleaseType level = ReleaseType.Auto)
    {
        for (int i = _releaseQueue.Count - 1; i >= 0; i--)
        {
            UINode node = _releaseQueue[i];
            if (node.ReleaseType <= level)
            {
                DestroyNode(node);
                _releaseQueue.RemoveAt(i);
            }
        }
    }

    public void AddNodeHideCallback(string name, Action<object?> callback)
    {
        if (!_hideCallbacks.TryGetValue(name, out List<Action<object?>>? callbacks))
        {
            callbacks = new List<Action<object?>>();
            _hideCallbacks[name] = callbacks;
        }

        if (!callbacks.Contains(callback))
        {
            callbacks.Add(callback);
        }
    }

    public void RemoveNodeHideCallback(string name, Action<object?> callback)
    {
        if (_hideCallbacks.TryGetValue(name, out List<Action<object?>>? callbacks))
        {
            callbacks.Remove(callback);
        }
    }

    public void DispatchNodeHide(string name, object? returnValue)
    {
        if (!_hideCallbacks.TryGetValue(name, out List<Action<object?>>? callbacks))
        {
            return;
        }

        foreach (Action<object?> callback in callbacks.ToArray())
        {
            callback.Invoke(returnValue);
        }
    }

    private void ShowElementInDomain(DomainNode domain, string elementName, object[] args)
    {
        if (_switchingNode != null && !ReferenceEquals(_switchingNode, domain))
        {
            return;
        }

        ElementNode element = GetOrCreateElement(elementName);
        element.SetDomainNode(domain);
        _switchingNode = element;

        if (element.ViewNode == null)
        {
            element.CreateElement(node =>
            {
                _uiRoot?.Attach(elementName, node);
                node.Create();
                SwitchNode(node, args);
            });
            return;
        }

        _uiRoot?.Attach(elementName, element);
        SwitchNode(element, args);
    }

    private void SwitchNode(UINode node, object[] args)
    {
        node.Switch(canOpen =>
        {
            if (!canOpen)
            {
                _switchingNode = null;
                return;
            }

            if (node is DomainNode domain)
            {
                PushDomain(domain);
            }

            node.Show(args);
            _switchingNode = null;
        });
    }

    private void PushDomain(DomainNode domain)
    {
        if (_domainStack.Contains(domain))
        {
            RemoveDomainFromStack(domain);
        }

        domain.SetStackIndex(_domainStack.Count);
        _domainStack.Push(domain);
        RefreshDomainCoverage();
    }

    private DomainNode GetOrCreateDomain(string name)
    {
        if (_domains.TryGetValue(name, out DomainNode? domain))
        {
            return domain;
        }

        domain = new DomainNode(name);
        domain.SetDomainNode(domain);
        domain.Construct();
        domain.Create();
        _domains[name] = domain;
        return domain;
    }

    private ElementNode GetOrCreateElement(string name)
    {
        if (_elements.TryGetValue(name, out ElementNode? element))
        {
            return element;
        }

        element = new ElementNode(name);
        element.Construct();
        _elements[name] = element;
        return element;
    }

    private void UpdateDomain(DomainNode? domain)
    {
        if (domain == null)
        {
            return;
        }

        _updateNodes.Clear();
        domain.GetAllChildNode(_updateNodes);
        foreach (UINode node in _updateNodes)
        {
            node.Update();
        }
    }

    private void UpdateReleaseQueue()
    {
        for (int i = _releaseQueue.Count - 1; i >= 0; i--)
        {
            UINode node = _releaseQueue[i];
            if (node.CanRelease())
            {
                DestroyNode(node);
                _releaseQueue.RemoveAt(i);
            }
        }
    }

    private void DestroyNode(UINode node)
    {
        switch (node)
        {
            case ElementNode element:
                _elements.Remove(element.NodeName);
                element.Destroy();
                break;
            case DomainNode domain:
                _domains.Remove(domain.NodeName);
                domain.Destroy();
                break;
        }
    }

    private void RefreshDomainCoverage()
    {
        DomainNode[] domains = _domainStack.Reverse().ToArray();
        for (int i = 0; i < domains.Length; i++)
        {
            domains[i].SetStackIndex(i);
            domains[i].Covered(i < domains.Length - 1);
        }
    }

    private void RegisterUILogics()
    {
        _uiLogics.Clear();
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetLoadableTypes))
        {
            if (type.IsAbstract || !typeof(IUILogic).IsAssignableFrom(type))
            {
                continue;
            }

            _uiLogics[type.Name] = type;
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
    }
}
