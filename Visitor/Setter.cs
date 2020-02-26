namespace Visitors
{
    using System.Linq;
    using System.Reflection;

    public static class Setter
    {
        public static ISetter<T, V> Create<T, V>(PropertyInfo property) =>
            property.GetMethod.DeclaringType.IsClass
                ? new ClassSetter<T, V>(property.SetMethod)
                : new StructSetter<T, V>(property.SetMethod)
                as ISetter<T, V>;

        public static ISetter<T, V> Create<T, V>(string propertyName) =>
            Create<T, V>(
                typeof(T)
                    .GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                    .Single(property => property.Name == propertyName));
    }

    public interface ISetter<T, V>
    {
        void Apply(in T obj, V value);
    }

    internal struct StructSetter<T, V> : ISetter<T, V>
    {
        private readonly SetterImpl impl;

        internal StructSetter(MethodInfo method) =>
            impl = (SetterImpl)method.CreateDelegate(typeof(SetterImpl));

        public void Apply(in T obj, V value)
        {
            impl(in obj, value);
        }

        private delegate void SetterImpl(in T obj, V value);
    }

    internal struct ClassSetter<T, V> : ISetter<T, V>
    {
        private readonly SetterImpl impl;

        internal ClassSetter(MethodInfo method) =>
            impl = (SetterImpl)method.CreateDelegate(typeof(SetterImpl));

        public void Apply(in T obj, V value)
        {
            impl(obj, value);
        }

        private delegate void SetterImpl(T obj, V value);
    }
}
