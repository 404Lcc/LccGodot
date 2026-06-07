using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace LccHotfix
{
    public static class AutoReferenceUtility
    {
        #region 自动索引

        public static void AutoReference(object obj, Node node)
        {
            Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
            FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type objectType = typeof(GodotObject);
            foreach (FieldInfo item in fieldInfos)
            {
                if (item.FieldType.IsSubclassOf(objectType))
                {
                    fieldInfoDict[item.Name.ToLower()] = item;
                }
            }

            if (fieldInfoDict.Count > 0)
            {
                AutoReference(obj, node, fieldInfoDict);
            }
        }

        public static void AutoReference(object obj, Node node, Dictionary<string, FieldInfo> fieldInfoDict)
        {
            string name = node.Name.ToString().ToLower();
            if (fieldInfoDict.ContainsKey(name))
            {
                if (fieldInfoDict[name].FieldType.IsInstanceOfType(node))
                {
                    fieldInfoDict[name].SetValue(obj, node);
                }
            }

            foreach (Node item in GetChildren(node))
            {
                string itemName = item.Name.ToString().ToLower();
                if (fieldInfoDict.ContainsKey(itemName))
                {
                    if (fieldInfoDict[itemName].FieldType.IsInstanceOfType(item))
                    {
                        fieldInfoDict[itemName].SetValue(obj, item);
                    }
                }
            }
        }

        private static List<Node> GetChildren(Node node)
        {
            List<Node> result = new List<Node>();
            CollectChildren(node, result);
            return result;
        }

        private static void CollectChildren(Node node, List<Node> list)
        {
            foreach (Node child in node.GetChildren())
            {
                list.Add(child);
                CollectChildren(child, list);
            }
        }

        #endregion
    }
}