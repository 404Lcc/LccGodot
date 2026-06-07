using Godot;

namespace LccModel
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        protected static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.OnInit();
                }

                return _instance;
            }
        }

        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                _instance.OnDestroy();
                _instance = null;
            }
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnDestroy()
        {
        }
    }

    public partial class SingletonNode<T> : Node where T : Node, new()
    {
        protected static T _instance = null;

        public static T Instance
        {
            get
            {
                if (Engine.GetMainLoop() is not SceneTree tree)
                {
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindInstance(tree.Root);
                }

                var root = tree.Root.GetNodeOrNull<Node>("SingletonNode");
                if (root == null)
                {
                    root = new Node { Name = "SingletonNode" };
                    tree.Root.AddChild(root);
                }

                if (_instance == null)
                {
                    _instance = new T { Name = typeof(T).ToString() };
                    root.AddChild(_instance);
                }

                return _instance;
            }
        }

        public static bool HaveInstance
        {
            get
            {
                if (Engine.GetMainLoop() is not SceneTree tree)
                {
                    return false;
                }

                var root = tree.Root.GetNodeOrNull<Node>("SingletonNode");
                if (root == null)
                {
                    return false;
                }

                var node = root.GetNodeOrNull<Node>(typeof(T).ToString());
                return node != null;
            }
        }

        private static T FindInstance(Node node)
        {
            if (node is T instance)
            {
                return instance;
            }

            foreach (var child in node.GetChildren())
            {
                var found = FindInstance(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}