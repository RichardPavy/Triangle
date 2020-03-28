namespace Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Parent class for class and field visitors.
    /// </summary>
    public abstract class Visitor
    {
        protected Visitor(MustVisitStatus mustVisit)
        {
            MustVisit = mustVisit;
        }

        protected abstract IEnumerable<Visitor> ChildVisitors();

        /// <summary>
        /// Whether this visitor must be traversed.
        /// </summary>
        internal readonly MustVisitStatus MustVisit;

        /// <summary>
        /// Whether this visitor should be traversed.
        /// - Because it is <see cref="MustVisit"/> == true, or
        /// - Because one of the <see cref="ChildVisitors"/> must be traversed.
        /// </summary>
        internal bool ShouldVisit()
        {
            ISet<Visitor> allVisitors = new HashSet<Visitor>();
            FindAllVisitors(allVisitors);
            return allVisitors.Any(visitor => visitor.MustVisit == MustVisitStatus.Yes);
        }

        private void FindAllVisitors(ISet<Visitor> accu)
        {
            if (MustVisit != MustVisitStatus.Never)
                if (accu.Add(this))
                    foreach (Visitor childVisitor in ChildVisitors())
                        childVisitor.FindAllVisitors(accu);
        }

        internal abstract void Initialize();
    }

    public enum MustVisitStatus
    {
        Yes, No, Never
    }

    public enum VisitStatus
    {
        Continue, SkipChildren, Exit
    }

    public struct VisitorScope
    {
        public readonly VisitStatus Status;
        public readonly Action After;

        private VisitorScope(VisitStatus status, Action after)
        {
            Status = status;
            After = after;
        }

        public static implicit operator VisitorScope(Action after) =>
            new VisitorScope(VisitStatus.Continue, after);
        public static implicit operator VisitorScope(VisitStatus status) =>
            new VisitorScope(status, null);
    }
}
