using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public interface ICodeTypesService : IService
    {
        void LoadTypes(params Assembly[] assemblies);
        Dictionary<string, Type> GetAssemblyTypes(params Assembly[] assemblies);
        HashSet<Type> GetTypes(Type attributeType);
        Dictionary<string, Type> GetTypes();
        Type? GetType(string typeName);
    }
}
