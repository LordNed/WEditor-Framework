using System;

namespace WEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RequireComponent : Attribute
    {
        public Type[] RequiredComponents { get; private set; }

        public RequireComponent(Type componentType)
        {
            RequiredComponents = new[] { componentType };
        }
    }
}
