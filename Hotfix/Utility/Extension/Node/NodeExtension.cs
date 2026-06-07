using Godot;

namespace LccHotfix
{
    public static class NodeExtension
    {
        public static void SetParent(this Node node, Node parent)
        {
            if (node == null)
            {
                return;
            }

            if (parent == null)
            {
                if (Engine.GetMainLoop() is not SceneTree tree)
                {
                    return;
                }
                
                parent = tree.Root;
            }

            if (node.GetParent() == null)
            {
                parent.AddChild(node);
            }
            else if (node.GetParent() != parent)
            {
                node.Reparent(parent);
            }
        }

        public static Vector3 GetPosition(this Node node)
        {
            if (node == null)
            {
                return Vector3.Zero;
            }

            if (node is Node2D node2D)
            {
                return new Vector3(node2D.Position.X, node2D.Position.Y, 0);
            }

            if (node is Node3D node3D)
            {
                return node3D.Position;
            }

            if (node is Control control)
            {
                return new Vector3(control.Position.X, control.Position.Y, 0);
            }

            return Vector3.Zero;
        }

        public static void SetPosition(this Node node, Vector3 position)
        {
            if (node == null)
            {
                return;
            }

            if (node is Node2D node2D)
            {
                node2D.Position = new Vector2(position.X, position.Y);
            }

            if (node is Node3D node3D)
            {
                node3D.Position = node3D.Position;
            }

            if (node is Control control)
            {
                control.Position = new Vector2(position.X, position.Y);
            }
        }

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