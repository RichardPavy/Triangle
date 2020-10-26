namespace Triangle.LevelDB.Iterators
{
    /// <summary>
    /// A DB iterator that loops in reverse order.
    /// </summary>
    internal sealed class ReverseIterator : DelegateIterator
    {
        internal ReverseIterator(AbstractIterator forwardIterator) : base(forwardIterator)
        {
        }

        internal override void Next()
        {
            base.Previous();
        }

        internal override void Previous()
        {
            base.Next();
        }

        internal override IIterator SeekToFirst()
        {
            base.SeekToLast();
            return this;
        }

        internal override IIterator SeekToLast()
        {
            base.SeekToFirst();
            return this;
        }

        internal override int CompareKeys(byte[] a, byte[] b) => -base.CompareKeys(a, b);

        public override IIterator Reverse() => delegateIterator;
        public override IIterator Range(byte[] from, byte[] to) => new RangeIterator(this, from, to);
    }
}
