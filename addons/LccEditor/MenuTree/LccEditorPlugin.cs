using Godot;

namespace LccEditor
{
    [Tool]
    public partial class LccEditorPlugin : EditorPlugin
    {
        private LccMenuEditorDock _dock;

        public override void _EnterTree()
        {
            _dock = new LccMenuEditorDock(this);
            AddDock(_dock);
        }

        public override void _ExitTree()
        {
            if (_dock == null)
            {
                return;
            }

            RemoveDock(_dock);

            _dock.QueueFree();
            _dock = null;
        }
    }
}