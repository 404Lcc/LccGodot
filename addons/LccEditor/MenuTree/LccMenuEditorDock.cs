using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LccEditor
{
    public class MenuTreeAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; }

        public MenuTreeAttribute(string name, int order = 0)
        {
            Name = name;
            Order = order;
        }
    }

    public class EditorWindowData
    {
        public Type Type { get; }
        public MenuTreeAttribute Attribute { get; }

        public EditorWindowData(Type type, MenuTreeAttribute attribute)
        {
            Type = type;
            Attribute = attribute;
        }
    }

    [Tool]
    public partial class LccMenuEditorDock : EditorDock
    {
        private EditorPlugin _editorPlugin;
        private Tree _menuTree;
        private PanelContainer _contentRoot;
        private Control _currentContent;

        private LccEditorWindowBase _currentWindow;

        private Dictionary<TreeItem, LccEditorWindowBase> _dict = new Dictionary<TreeItem, LccEditorWindowBase>();

        public EditorPlugin EditorPlugin => _editorPlugin;

        public LccMenuEditorDock(EditorPlugin editorPlugin)
        {
            _editorPlugin = editorPlugin;
            Name = "LccEditor";
            Title = "LccEditor";
            LayoutKey = "LccEditor";
            DefaultSlot = DockSlot.LeftUl;
            AvailableLayouts = DockLayout.All;
        }

        public override void _Ready()
        {
            BuildLayout();
        }

        public override void _ExitTree()
        {
            if (_currentWindow != null)
            {
                _currentWindow.OnDisable();
            }

            ClearContent();
            _dict.Clear();
        }

        public void SelectWindow<T>() where T : LccEditorWindowBase
        {
            TreeItem item = _dict.FirstOrDefault(pair => pair.Value.GetType() == typeof(T)).Key;
            if (item == null)
            {
                return;
            }

            item.Select(0);
            SelectWindow(_dict[item]);
        }

        private void BuildLayout()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            SizeFlagsVertical = SizeFlags.ExpandFill;

            var split = new HSplitContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,
            };

            _menuTree = new Tree
            {
                HideRoot = true,
                CustomMinimumSize = new Vector2(180, 0),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,
            };
            _menuTree.ItemSelected += OnMenuItemSelected;
            split.AddChild(_menuTree);


            _contentRoot = new PanelContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill,
            };
            split.AddChild(_contentRoot);

            AddChild(split);


            TreeItem root = _menuTree.CreateItem();

            foreach (EditorWindowData data in FindEditorWindows())
            {
                var window = (LccEditorWindowBase)Activator.CreateInstance(data.Type)!;
                window.Initialize(this);

                TreeItem item = GetOrCreateMenuItem(root, data.Attribute.Name);
                _dict.Add(item, window);
            }

            if (_dict.Count > 0)
            {
                SelectWindow(_dict.First().Value);
            }
        }

        private List<EditorWindowData> FindEditorWindows()
        {
            List<EditorWindowData> list = new List<EditorWindowData>();

            foreach (Type type in typeof(LccMenuEditorDock).Assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(LccEditorWindowBase).IsAssignableFrom(type))
                {
                    continue;
                }

                MenuTreeAttribute attribute = type.GetCustomAttribute<MenuTreeAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                list.Add(new EditorWindowData(type, attribute));
            }

            return list.OrderBy(item => item.Attribute.Order).ThenBy(item => item.Attribute.Name, StringComparer.Ordinal).ToList();
        }

        private TreeItem GetOrCreateMenuItem(TreeItem root, string menuPath)
        {
            TreeItem parent = root;

            foreach (string name in menuPath.Split('/'))
            {
                parent = GetOrCreateChild(parent, name);
            }

            return parent;
        }

        private TreeItem GetOrCreateChild(TreeItem parent, string name)
        {
            TreeItem item = parent.GetFirstChild();

            while (item != null)
            {
                if (item.GetText(0) == name)
                {
                    return item;
                }

                item = item.GetNext();
            }

            item = parent.CreateChild();
            item.SetText(0, name);
            return item;
        }

        private void OnMenuItemSelected()
        {
            TreeItem item = _menuTree.GetSelected();
            if (item != null && _dict.TryGetValue(item, out var window))
            {
                SelectWindow(window);
            }
        }

        private void SelectWindow(LccEditorWindowBase window)
        {
            if (_currentWindow == window)
            {
                return;
            }

            if (_currentWindow != null)
            {
                _currentWindow.OnDisable();
            }

            _currentWindow = window;
            _currentWindow.OnEnable();
            _currentWindow.OnSelected();

            SetContent(_currentWindow.BuildContent());
        }

        private void SetContent(Control content)
        {
            ClearContent();

            _currentContent = content;
            _currentContent.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _currentContent.SizeFlagsVertical = SizeFlags.ExpandFill;
            _contentRoot.AddChild(_currentContent);
        }

        private void ClearContent()
        {
            if (_currentContent == null)
            {
                return;
            }

            _currentContent.QueueFree();
            _currentContent = null;
        }
    }
}