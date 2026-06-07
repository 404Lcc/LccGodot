using Godot;

namespace LccHotfix
{
    public static class NodeExtension
    {
        public static void SetActive(this Node node, bool active)
        {
            if (node == null)
            {
                return;
            }

            node.ProcessMode = active ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;

            if (node is Node2D node2D)
            {
                node2D.Visible = active;
            }

            if (node is Node3D node3D)
            {
                node3D.Visible = active;
            }

            if (node is Control control)
            {
                control.Visible = active;
            }
        }
    }
}