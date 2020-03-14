namespace Visitors
{
    using System;

    public sealed class VisitorProcessor
    {
        internal Delegate Process { get; }
        internal MustVisitStatus MustVisit { get; }

        private VisitorProcessor(Delegate process)
        {
            Process = process;
            MustVisit = process == null ? MustVisitStatus.No : MustVisitStatus.Yes;
        }

        private VisitorProcessor(MustVisitStatus mustVisit)
        {
            Process = null;
            MustVisit = mustVisit;
        }

        public static implicit operator VisitorProcessor(Delegate process) => new VisitorProcessor(process);
        public static implicit operator VisitorProcessor(MustVisitStatus mustVisit) => new VisitorProcessor(mustVisit);

    }

    public enum MustVisitStatus
    {
        Yes, No, Never
    }

    public delegate void Process<TData, TObj>(TData data, TObj obj);
}
