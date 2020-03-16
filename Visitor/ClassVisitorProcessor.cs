namespace Visitors
{
    using System;

    public sealed class ClassVisitorProcessor
    {
        internal Delegate Process { get; }
        internal MustVisitStatus MustVisit { get; }

        private ClassVisitorProcessor(Delegate process)
        {
            Process = process;
            MustVisit = process == null ? MustVisitStatus.No : MustVisitStatus.Yes;
        }

        private ClassVisitorProcessor(MustVisitStatus mustVisit)
        {
            Process = null;
            MustVisit = mustVisit;
        }

        public static implicit operator ClassVisitorProcessor(Delegate process) => new ClassVisitorProcessor(process);
        public static implicit operator ClassVisitorProcessor(MustVisitStatus mustVisit) => new ClassVisitorProcessor(mustVisit);

    }

    public delegate VisitStatus ProcessObject<TData, TObj>(TData data, TObj obj);
}
