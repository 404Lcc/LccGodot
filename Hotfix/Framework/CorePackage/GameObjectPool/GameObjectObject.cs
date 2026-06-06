using Godot;

namespace LccHotfix
{
    public class GameObjectObject
    {
        private IGameObjectPool _pool;
        private Node _gameObject;

        public IGameObjectPool Pool
        {
            get { return _pool; }
            set { _pool = value; }
        }

        public Node GameObject => _gameObject;
        public Node Transform => GameObject;

        public GameObjectObject(Node gameObject)
        {
            _gameObject = gameObject;
        }

        public void Release(ref GameObjectObject obj)
        {
            _pool.Release(this);
            obj = null;
        }

        public void OnReset()
        {
            if (GameObject is Node2D node2D)
            {
                node2D.Position = new Vector2(30000, 0);
            }

            if (GameObject is Node3D node3D)
            {
                node3D.Position = new Vector3(30000, 0, 0);
            }

            if (GameObject is Control control)
            {
                control.Position = new Vector2(30000, 0);
            }
        }

        public void SetActive(bool active)
        {
            GameObject.ProcessMode = active ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;

            if (GameObject is Node2D node2D)
            {
                node2D.Visible = active;
            }

            if (GameObject is Node3D node3D)
            {
                node3D.Visible = active;
            }

            if (GameObject is Control control)
            {
                control.Visible = active;
            }
        }
    }
}