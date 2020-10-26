namespace Triangle.Visitors
{
    using System;
    using Triangle.Visitors.Utils;

    public struct VisitorScope<TData>
    {
        public readonly VisitStatus Status;
        public readonly Action After;
        public readonly Optional<TData> Data;

        private VisitorScope(VisitStatus status, Action after)
        {
            Status = status;
            After = after;
            Data = Optional<TData>.Null;
        }

        private VisitorScope(VisitStatus status, Action after, Optional<TData> data)
        {
            Status = status;
            After = after;
            Data = data;
        }

        public static implicit operator VisitorScope<TData>(Action after) =>
            new VisitorScope<TData>(VisitStatus.Continue, after);
        public static implicit operator VisitorScope<TData>(VisitStatus status) =>
            new VisitorScope<TData>(status, null);

        public static VisitorScope<TData> operator +(VisitorScope<TData> scope, Action andThen)
        {
            return new VisitorScope<TData>(scope.Status, scope.After + andThen, scope.Data);
        }

        public static VisitorScope<TData> operator +(VisitorScope<TData> scope, Optional<TData> data)
        {
            return new VisitorScope<TData>(scope.Status, scope.After, data);
        }
    }
}
