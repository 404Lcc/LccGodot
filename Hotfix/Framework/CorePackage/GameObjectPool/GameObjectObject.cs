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
            GameObject.SetPosition(new Vector3(30000, 0, 0));
        }
    }
}