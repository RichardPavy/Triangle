namespace Triangle.Visitors
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
        private readonly ProcessObject<TData, TObj> process;

        // Faster access than Lazy fields.
        private FieldVisitor<TData, TObj>[] enabledFieldVisitorsCache = null;

        internal ClassVisitor(
            VisitorFactory<TData> visitorFactory,
            ProcessObject<TData, TObj> process,
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
            {
                enabledFieldVisitorsCache = enabledFieldVisitors.Value.ToArray();
                foreach (var fieldVisitor in enabledFieldVisitorsCache)
                    fieldVisitor.Initialize();
            }
        }

        public VisitStatus Visit(TData data, TObj obj)
        {
            if (MustVisit != MustVisitStatus.Yes)
                return VisitFields(data, obj, VisitStatus.Continue);
            VisitorScope<TData> scope = process(data, obj);
            VisitStatus result = scope.Status == VisitStatus.Continue
                ? VisitFields(data, obj, scope.Status)
                : scope.Status;
            scope.After?.Invoke();
            return result;
        }

        private VisitStatus VisitFields(
            TData data,
            TObj obj,
            VisitStatus defaultStatus)
        {
            foreach (var fieldVisitor in enabledFieldVisitorsCache)
                if (fieldVisitor.Visit(data, obj) == VisitStatus.Exit)
                    return VisitStatus.Exit;
            return defaultStatus;
        }

        protected override IEnumerable<Visitor> ChildVisitors() =>
            allFieldVisitors.Value;
    }
}
