using System;

namespace Ben.Utilities
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ViewTypeAttribute : Attribute
    {
        public ViewTypeAttribute(Type viewType)
        {
            this.ViewType = viewType;
        }

        public Type ViewType { get; private set; }
    }
}
