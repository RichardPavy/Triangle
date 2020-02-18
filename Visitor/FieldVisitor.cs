namespace Visitors
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    internal abstract class FieldVisitor<TData, TObj> : Visitor
    {
        internal abstract void Visit(TData data, TObj obj);
    }

    internal sealed class FieldVisitor<TData, TObj, V> : FieldVisitor<TData, TObj>
    {
        private readonly PropertyInfo property;
        private readonly ClassVisitor<TData, V> classVisitor;
        private readonly Process<TData, V> process;

        private IGetter getter;

        internal FieldVisitor(
            VisitorFactory<TData> visitorFactory,
            PropertyInfo property,
            Process<TData, V> process)
        {
            this.property = property;
            this.classVisitor = visitorFactory.GetClassVisitor<V>();
            this.process = process;
        }

        internal override void Visit(TData data, TObj obj)
        {
            V value = getter.Apply(obj);
            if (MustVisit()) process(data, value);
            classVisitor.Visit(data, value);
        }

        protected override IEnumerable<Visitor> ChildVisitors() =>
            ImmutableArray.Create(classVisitor);

        internal override bool MustVisit() => process != null;

        internal override void Initialize()
        {
            getter = property.GetMethod.DeclaringType.IsClass
                ? new ClassGetter(property.GetMethod)
                : new StructGetter(property.GetMethod)
                as IGetter;
        }

        internal interface IGetter
        {
            V Apply(in TObj obj);
        }

        internal struct StructGetter : IGetter
        {
            private readonly GetterImpl impl;

            internal StructGetter(MethodInfo method) =>
                impl = (GetterImpl)method.CreateDelegate(typeof(GetterImpl));

            public V Apply(in TObj obj) => impl(in obj);

            private delegate V GetterImpl(in TObj obj);
        }

        internal struct ClassGetter : IGetter
        {
            private readonly GetterImpl impl;

            internal ClassGetter(MethodInfo method) =>
                impl = (GetterImpl)method.CreateDelegate(typeof(GetterImpl));

            public V Apply(in TObj obj) => impl(obj);

            private delegate V GetterImpl(TObj obj);
        }
    }
}
