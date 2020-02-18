namespace Visitors
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Parent class for class and field visitors.
    /// </summary>
    public abstract class Visitor
    {
        protected abstract IEnumerable<Visitor> ChildVisitors();

        /// <summary>
        /// Whether this visitor must be traversed.
        /// </summary>
        internal abstract bool MustVisit();

        /// <summary>
        /// Whether this visitor should be traversed.
        /// - Because it is <see cref="MustVisit"/> == true, or
        /// - Because one of the <see cref="ChildVisitors"/> must be traversed.
        /// </summary>
        internal bool ShouldVisit()
        {
            ISet<Visitor> allVisitors = new HashSet<Visitor>();
            FindAllVisitors(allVisitors);
            return allVisitors.Any(visitor => visitor.MustVisit());
        }

        private void FindAllVisitors(ISet<Visitor> accu)
        {
            if (accu.Add(this))
                foreach (Visitor childVisitor in ChildVisitors())
                    childVisitor.FindAllVisitors(accu);
        }

        internal abstract void Initialize();

        public delegate void Process<TData, TObj>(TData data, TObj obj);
    }
}
