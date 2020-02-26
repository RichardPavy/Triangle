namespace Visitors
{
    using System.Linq;
    using System.Reflection;

    public static class Getter
    {
        public static IGetter<T, V> Create<T, V>(PropertyInfo property) =>
            property.GetMethod.DeclaringType.IsClass
                ? new ClassGetter<T, V>(property.GetMethod)
                : new StructGetter<T, V>(property.GetMethod)
                as IGetter<T, V>;

        public static IGetter<T, V> Create<T, V>(string propertyName) =>
            Create<T, V>(
                typeof(T)
                    .GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .Single(property => property.Name == propertyName));
    }

    public interface IGetter<T, V>
    {
        V Apply(in T obj);
    }

    internal struct StructGetter<T, V> : IGetter<T, V>
    {
        private readonly GetterImpl impl;

        internal StructGetter(MethodInfo method) =>
            impl = (GetterImpl)method.CreateDelegate(typeof(GetterImpl));

        public V Apply(in T obj)
        {
            return impl(in obj);
        }

        private delegate V GetterImpl(in T obj);
    }

    internal struct ClassGetter<T, V> : IGetter<T, V>
    {
        private readonly GetterImpl impl;

        internal ClassGetter(MethodInfo method) =>
            impl = (GetterImpl)method.CreateDelegate(typeof(GetterImpl));

        public V Apply(in T obj)
        {
            return impl(obj);
        }

        private delegate V GetterImpl(T obj);
    }
}
