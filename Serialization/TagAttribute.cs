namespace Triangle.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class TagAttribute : Attribute
    {
        public readonly int Tag;

        public TagAttribute(int tag)
        {
            if (tag == 0)
            {
                throw new ArgumentException($"Invalid tag: {tag}");
            }
            this.Tag = tag;
        }
    }

    internal static class TagExtensions
    {
        internal static IEnumerable<PropertyInfo> SerializableFields(this Type type) =>
            (from property in
                 type.GetProperties(
                     BindingFlags.FlattenHierarchy
                     | BindingFlags.Instance
                     | BindingFlags.Public
                     | BindingFlags.NonPublic)
             where property.GetCustomAttributes<TagAttribute>().Any()
             select property);
    }
}
