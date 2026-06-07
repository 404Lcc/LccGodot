using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace LccHotfix
{
    public interface IGameObjectPool
    {
        Node Root { get; }
        GameObjectPoolSetting PoolSetting { get; }
        string Name { get; }
        int Count { get; }

        void Update();
        GameObjectObject Get();
        void Release(GameObjectObject obj);
        void ReleaseAll();
        GameObjectObject ForceSpawm();
        void ForceRelease();
    }

    public class GameObjectPool : IGameObjectPool
    {
        private PackedScene _original;
        private Node _root;
        private GameObjectPoolSetting _poolSetting;
        private Stack<GameObjectObject> _cachedStack;

        public Node Root => _root;
        public GameObjectPoolSetting PoolSetting => _poolSetting;
        public string Name { get; private set; }
        public int Count => _cachedStack.Count;

        public GameObjectPool(PackedScene original, Node root, GameObjectPoolSetting poolSetting, string location)
        {
            Debug.Assert(original != null);
            _original = original;
            _root = root;
            _poolSetting = poolSetting;
            Name = location;
            _cachedStack = new Stack<GameObjectObject>();
        }

        public virtual GameObjectObject Get()
        {
            GameObjectObject obj = null;
            if (_cachedStack.Count > 0)
            {
                obj = _cachedStack.Pop();
                SetParentToSceneRoot(obj.GameObject);
                obj.GameObject.SetActive(true);

                obj.OnReset();
                obj.Pool = this;
            }
            else
            {
                obj = ForceSpawm();
            }

            return obj;
        }

        public void Release(GameObjectObject obj)
        {
            if (obj != null)
            {
                obj.GameObject.SetActive(false);
                _cachedStack.Push(obj);
            }
        }

        public void ReleaseAll()
        {
            while (_cachedStack.Count > 0)
            {
                _cachedStack.Pop().GameObject.QueueFree();
            }

            _root.QueueFree();
        }

        public void Update()
        {
        }

        public GameObjectObject ForceSpawm()
        {
            var go = _original.Instantiate();
            _root.AddChild(go);
            var obj = new GameObjectObject(go);
            obj.GameObject.Name = Name;
            SetParentToSceneRoot(obj.GameObject);
            obj.GameObject.SetActive(true);

            obj.OnReset();
            obj.Pool = this;
            return obj;
        }

        public void ForceRelease()
        {
            if (_cachedStack.Count > 0)
            {
                _cachedStack.Pop().GameObject.QueueFree();
            }
        }

        private void SetParentToSceneRoot(Node node)
        {
            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                return;
            }

            if (node.GetParent() == tree.Root)
            {
                return;
            }

            node.Reparent(tree.Root);
        }
    }
}