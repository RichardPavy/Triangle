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

        internal abstract VisitStatus Visit(TData data, TObj obj);
    }

    internal sealed class FieldVisitor<TData, TObj, V> : FieldVisitor<TData, TObj>
    {
        private readonly PropertyInfo property;
        private readonly ClassVisitor<TData, V> classVisitor;
        private readonly ProcessField<TData, TObj, V> process;

        private IGetter<TObj, V> getter;

        internal FieldVisitor(
            VisitorFactory<TData> visitorFactory,
            PropertyInfo property,
            ProcessField<TData, TObj, V> process,
            MustVisitStatus mustVisit) : base(mustVisit)
        {
            this.property = property;
            this.classVisitor = visitorFactory.GetClassVisitor<V>();
            this.process = process;
        }

        internal override VisitStatus Visit(TData data, TObj obj)
        {
            V value = getter.Apply(obj);

            VisitStatus visitStatus =
                MustVisit == MustVisitStatus.Yes
                    ? process(data, obj, value)
                    : VisitStatus.Continue;

            if (visitStatus == VisitStatus.Continue)
            {
                if (classVisitor.Visit(data, value) == VisitStatus.Exit)
                {
                    return VisitStatus.Exit;
                }
            }

            return visitStatus;
        }

        protected override IEnumerable<Visitor> ChildVisitors() =>
            ImmutableArray.Create(classVisitor);

        internal override void Initialize()
        {
            getter = Getter.Create<TObj, V>(this.property);
        }
    }
}
