using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    internal sealed class CodeTypesManager : Module, ICodeTypesService
    {
        private readonly Dictionary<string, Type> _allTypes = new();
        private readonly Dictionary<Type, HashSet<Type>> _attributeTypes = new();

        public void LoadTypes(params Assembly[] assemblies)
        {
            foreach (var pair in GetAssemblyTypes(assemblies))
            {
                _allTypes[pair.Key] = pair.Value;

                if (pair.Value.IsAbstract)
                {
                    continue;
                }

                foreach (var attribute in pair.Value.GetCustomAttributes(typeof(AttributeBase), true))
                {
                    var attributeType = attribute.GetType();
                    if (!_attributeTypes.TryGetValue(attributeType, out var set))
                    {
                        set = new HashSet<Type>();
                        _attributeTypes[attributeType] = set;
                    }

                    set.Add(pair.Value);
                }
            }
        }

        public Dictionary<string, Type> GetAssemblyTypes(params Assembly[] assemblies)
        {
            var result = new Dictionary<string, Type>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName != null)
                    {
                        result[type.FullName] = type;
                    }
                }
            }

            return result;
        }

        public HashSet<Type> GetTypes(Type attributeType)
        {
            return _attributeTypes.TryGetValue(attributeType, out var set) ? new HashSet<Type>(set) : new HashSet<Type>();
        }

        public Dictionary<string, Type> GetTypes()
        {
            return new Dictionary<string, Type>(_allTypes);
        }

        public Type? GetType(string typeName)
        {
            _allTypes.TryGetValue(typeName, out var type);
            return type;
        }

        internal override void Shutdown()
        {
            _allTypes.Clear();
            _attributeTypes.Clear();
        }
    }
}
