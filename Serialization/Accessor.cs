namespace Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class Getter<T, V>
    {
        public static Func<T, V> Create(PropertyInfo property)
        {
            return (Func<T, V>)property.GetMethod.CreateDelegate(typeof(Func<T, V>));
        }

        public static Func<T, V> Create(string name)
        {
            return Create(Accessor.FindProperty(typeof(T), name));
        }
    }

    public static class Setter<T, V>
    {
        public static Action<T, V> Create(PropertyInfo property)
        {
            return (Action<T, V>)property.SetMethod.CreateDelegate(typeof(Action<T, V>));
        }

        public static Action<T, V> Create(string name)
        {
            return Create(Accessor.FindProperty(typeof(T), name));
        }
    }

    public class Accessor
    {
        internal static PropertyInfo FindProperty(Type type, string name) =>
            (from property in type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic)
             where property.Name == name
             select property).Single();

    }
}
