namespace Visitors
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Collections.Immutable;

    public sealed class ClassVisitor<TData, TObj> : Visitor
    {
        private readonly Lazy<ImmutableArray<FieldVisitor<TData, TObj>>> allFieldVisitors;
        private readonly Lazy<ImmutableArray<FieldVisitor<TData, TObj>>> enabledFieldVisitors;
        private readonly Process<TData, TObj> process;

        // Faster access than Lazy fields.
        private FieldVisitor<TData, TObj>[] enabledFieldVisitorsCache = null;

        internal ClassVisitor(
            VisitorFactory<TData> visitorFactory,
            Process<TData, TObj> process,
            MustVisitStatus mustVisit) : base(mustVisit)
        {
            allFieldVisitors =
                new Lazy<ImmutableArray<FieldVisitor<TData, TObj>>>(() =>
                    (from property in typeof(TObj).GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                     where property.CanRead
                     let method = property.GetMethod
                     where !method.MethodImplementationFlags.HasFlag(MethodImplAttributes.InternalCall)
                     select visitorFactory.CreateFieldVisitor<TObj>(property))
                     .ToImmutableArray());
            enabledFieldVisitors =
                new Lazy<ImmutableArray<FieldVisitor<TData, TObj>>>(() =>
                    allFieldVisitors.Value
                        .Where(fieldVisitor => fieldVisitor.ShouldVisit())
                        .ToImmutableArray());
            this.process = process;
        }

        internal override void Initialize()
        {
            if (enabledFieldVisitorsCache == null)
                enabledFieldVisitorsCache = enabledFieldVisitors.Value.ToArray();
            foreach (var fieldVisitor in enabledFieldVisitorsCache)
                fieldVisitor.Initialize();
        }

        public void Visit(TData data, TObj obj)
        {
            if (MustVisit == MustVisitStatus.Yes) process(data, obj);

            foreach (var fieldVisitor in enabledFieldVisitorsCache)
                fieldVisitor.Visit(data, obj);
        }

        protected override IEnumerable<Visitor> ChildVisitors() =>
            allFieldVisitors.Value;
    }
}
