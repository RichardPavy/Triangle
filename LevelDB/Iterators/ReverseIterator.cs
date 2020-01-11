namespace LevelDB.Iterators
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

        internal override IIterator SeekToFirst() => base.SeekToLast();
        internal override IIterator SeekToLast() => base.SeekToFirst();
        internal override int CompareKeys(string a, string b) => -base.CompareKeys(a, b);

        public override IIterator Reverse() => delegateIterator;
        public override IIterator Range(string from, string to) => new RangeIterator(this, from, to);
    }
}
