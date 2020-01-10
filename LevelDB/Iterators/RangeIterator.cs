namespace LevelDB
{
    using System;

    /// <summary>
    /// DB Iterator -- same as <see cref="Iterator"/>, but reverse.
    /// </summary>
    public sealed class RangeIterator<TBaseIterator> :
        DelegateIterator<
            RangeIterator<TBaseIterator>,
            RangeIterator<TBaseIterator>,
            TBaseIterator>
        where TBaseIterator : IIterator<TBaseIterator>
    {
        private readonly string from;
        private readonly string to;

        public RangeIterator(TBaseIterator delegateIterator, string from, string to) : base(delegateIterator)
        {
            this.from = from;
            this.to = to;
        }

        public override bool MoveNext()
        {
            return delegateIterator.MoveNext() && Key.CompareTo(to) < 0;
        }

        public override bool MovePrevious()
        {
            return delegateIterator.MovePrevious() && from.CompareTo(Key) <= 0;
        }

        public override RangeIterator<TBaseIterator> Reverse()
        {
            throw new NotImplementedException();
        }

        public override RangeIterator<TBaseIterator> Seek(string key)
        {
            if (from.CompareTo(key) <= 0 && key.CompareTo(to) <= 0)
            {
                delegateIterator.Seek(key);
                return this;
            }
            throw new ArgumentOutOfRangeException($"key '{key}' is not between ['{from}' - {to}]");
        }

        public override RangeIterator<TBaseIterator> SeekToFirst()
        {
            delegateIterator.Seek(this.from);
            return this;
        }

        public override RangeIterator<TBaseIterator> SeekToLast()
        {
            delegateIterator.Seek(this.to);
            return this;
        }
    }
}
