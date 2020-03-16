namespace Visitors
{
    using System;

    public sealed class FieldVisitorProcessor
    {
        internal Delegate Process { get; }
        internal MustVisitStatus MustVisit { get; }

        private FieldVisitorProcessor(Delegate process)
        {
            Process = process;
            MustVisit = process == null ? MustVisitStatus.No : MustVisitStatus.Yes;
        }

        private FieldVisitorProcessor(MustVisitStatus mustVisit)
        {
            Process = null;
            MustVisit = mustVisit;
        }

        public static implicit operator FieldVisitorProcessor(Delegate process) => new FieldVisitorProcessor(process);
        public static implicit operator FieldVisitorProcessor(MustVisitStatus mustVisit) => new FieldVisitorProcessor(mustVisit);

    }

    public delegate VisitStatus ProcessField<TData, TObj, TValue>(TData data, TObj obj, TValue value);
}
