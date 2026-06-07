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

    public static class AutoReferenceUtility
    {
        public static void AutoReference(object obj, Node transform)
        {
            Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
            FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type objectType = typeof(GodotObject);
            foreach (FieldInfo item in fieldInfos)
            {
                if (objectType.IsAssignableFrom(item.FieldType))
                {
                    fieldInfoDict[item.Name.ToLower()] = item;
                }
            }

            if (fieldInfoDict.Count > 0)
            {
                AutoReference(obj, transform, fieldInfoDict);
            }
        }

        public static void AutoReference(object obj, Node transform, Dictionary<string, FieldInfo> fieldInfoDict)
        {
            TrySetField(obj, transform, fieldInfoDict);

            foreach (Node item in GetChildrenRecursive(transform))
            {
                TrySetField(obj, item, fieldInfoDict);
            }
        }

        private static void TrySetField(object obj, Node node, Dictionary<string, FieldInfo> fieldInfoDict)
        {
            string name = node.Name.ToString().ToLower();
            if (!fieldInfoDict.TryGetValue(name, out var fieldInfo))
            {
                return;
            }

            if (fieldInfo.FieldType.IsInstanceOfType(node))
            {
                fieldInfo.SetValue(obj, node);
            }
        }

        private static IEnumerable<Node> GetChildrenRecursive(Node node)
        {
            foreach (Node child in node.GetChildren())
            {
                yield return child;

                foreach (Node item in GetChildrenRecursive(child))
                {
                    yield return item;
                }
            }
        }
    }
}
