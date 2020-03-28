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

            if (MustVisit != MustVisitStatus.Yes)
                return VisitField(data, value, VisitStatus.Continue);

            VisitorScope scope = process(data, obj, value);
            if (scope.Status == VisitStatus.Continue)
            {
                if (scope.After != null)
                {
                    try
                    {
                        return VisitField(data, value, scope.Status);
                    }
                    finally
                    {
                        scope.After();
                    }
                }

                return VisitField(data, value, scope.Status);
            }

            return scope.Status;

        }

        private VisitStatus VisitField(
            TData data,
            V value,
            VisitStatus defaultStatus)
        {
            return classVisitor.Visit(data, value) == VisitStatus.Exit
                ? VisitStatus.Exit
                : defaultStatus;
        }

        protected override IEnumerable<Visitor> ChildVisitors() =>
            ImmutableArray.Create(classVisitor);

        internal override void Initialize()
        {
            getter = Getter.Create<TObj, V>(this.property);
        }
    }
}
