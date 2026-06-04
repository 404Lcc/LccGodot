using System;

namespace LccHotfix
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public abstract class AttributeBase : Attribute
    {
    }
}
