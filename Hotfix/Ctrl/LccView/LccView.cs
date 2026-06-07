using Godot;

namespace LccHotfix
{
    public partial class LccView : Node
    {
        public string className;
        public object type;

        public T GetType<T>()
        {
            return (T)type;
        }
    }
}