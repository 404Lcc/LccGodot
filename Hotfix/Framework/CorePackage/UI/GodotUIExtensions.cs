using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace LccHotfix
{
    public static class GodotUIExtensions
    {
        public static void SetActive(this Node node, bool active)
        {
            if (node == null)
            {
                return;
            }

            node.ProcessMode = active ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;

            if (node is CanvasItem canvasItem)
            {
                canvasItem.Visible = active;
            }
        }
    }

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
