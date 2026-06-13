using Godot;

namespace LccEditor
{
    public abstract class LccEditorWindowBase
    {
        public LccMenuEditorDock Dock { get; private set; }
        public EditorPlugin EditorPlugin => Dock.EditorPlugin;

        internal void Initialize(LccMenuEditorDock dock)
        {
            Dock = dock;
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnSelected()
        {
        }

        public abstract Control BuildContent();
    }
}