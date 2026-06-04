using Godot;

namespace LccHotfix
{
    public partial class GodotMainBridge : Node
    {
        public override void _Ready()
        {
            if (Main.Current == null)
            {
                Main.SetMain(new SimpleFrameworkMain());
            }
        }

        public override void _Process(double delta)
        {
            Main.Tick((float)delta, (float)delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            Main.LateTick();
        }

        public override void _ExitTree()
        {
            Main.Close();
        }
    }
}
