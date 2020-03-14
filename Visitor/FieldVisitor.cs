namespace Visitors
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    internal abstract class FieldVisitor<TData, TObj> : Visitor
    {
        protected FieldVisitor(MustVisitStatus mustVisit) : base(mustVisit)
        {
        }

        internal abstract void Visit(TData data, TObj obj);
    }

    internal sealed class FieldVisitor<TData, TObj, V> : FieldVisitor<TData, TObj>
    {
        private readonly PropertyInfo property;
        private readonly ClassVisitor<TData, V> classVisitor;
        private readonly Process<TData, V> process;

        private IGetter<TObj, V> getter;

        internal FieldVisitor(
            VisitorFactory<TData> visitorFactory,
            PropertyInfo property,
            Process<TData, V> process,
            MustVisitStatus mustVisit) : base(mustVisit)
        {
            this.property = property;
            this.classVisitor = visitorFactory.GetClassVisitor<V>();
            this.process = process;
        }

        internal override void Visit(TData data, TObj obj)
        {
            V value = getter.Apply(obj);
            if (MustVisit == MustVisitStatus.Yes) process(data, value);
            classVisitor.Visit(data, value);
        }

        protected override IEnumerable<Visitor> ChildVisitors() =>
            ImmutableArray.Create(classVisitor);

        internal override void Initialize()
        {
            getter = Getter.Create<TObj, V>(this.property);
        }
    }
}
