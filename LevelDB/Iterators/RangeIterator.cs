namespace LevelDB.Iterators
{
    using System;

    /// <summary>
    /// DB Iterator -- same as <see cref="Iterator"/>, but reverse.
    /// </summary>
    internal sealed class RangeIterator : DelegateIterator
    {
        private readonly string from;
        private readonly string to;

        public RangeIterator(IIterator delegateIterator, string from, string to) : base(delegateIterator)
        {
            if (from.CompareTo(to) > 0)
            {
                throw new ArgumentException($"Invalid range ['{from}' - {to}]");
            }
            this.from = from;
            this.to = to;
        }

        public override bool MoveNext()
        {
            return delegateIterator.MoveNext() && Key.CompareTo(this.to) < 0;
        }

        public override bool MovePrevious()
        {
            return delegateIterator.MovePrevious() && this.from.CompareTo(Key) <= 0;
        }

        public override IIterator Seek(string key)
        {
            if (this.from.CompareTo(key) <= 0 && key.CompareTo(this.to) <= 0)
            {
                delegateIterator.Seek(key);
                return this;
            }
            throw new ArgumentOutOfRangeException($"key '{key}' is not between ['{this.from}' - {this.to}]");
        }

        public override IIterator SeekToFirst()
        {
            delegateIterator.Seek(this.from);
            return this;
        }

        public override IIterator SeekToLast()
        {
            delegateIterator.Seek(this.to);
            return this;
        }

        public override IIterator Range(string from, string to)
        {
            if (this.from.CompareTo(from) <= 0 && to.CompareTo(this.to) <= 0)
            {
                return new RangeIterator(delegateIterator, from, to);
            }
            throw new ArgumentOutOfRangeException($"['{from}' - {to}] is not included in ['{this.from}' - {this.to}]");
        }
    }
}
