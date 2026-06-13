using Godot;

namespace LccEditor
{
    [MenuTree("Lcc框架/概览", 0)]
    public sealed class FrameworkEditorWindow : LccEditorWindowBase
    {
        public override Control BuildContent()
        {
            var root = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(360, 200),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            };

            root.AddChild(new Label
            {
                Text = "LccEditor",
                HorizontalAlignment = HorizontalAlignment.Left,
            });

            root.AddChild(new Label
            {
                Text = "Lcc框架",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            });

            var openFolderButton = new Button
            {
                Text = "打开插件目录",
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
            };
            openFolderButton.Pressed += OnOpenFolderButtonPressed;
            root.AddChild(openFolderButton);

            return root;
        }

        private void OnOpenFolderButtonPressed()
        {
            string path = ProjectSettings.GlobalizePath("res://addons/LccEditor");
            OS.ShellOpen(path);
        }
    }
}