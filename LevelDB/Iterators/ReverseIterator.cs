namespace LevelDB.Iterators
{
    /// <summary>
    /// Reverse iterator.
    /// </summary>
    internal sealed class ReverseIterator : DelegateIterator
    {
        internal ReverseIterator(IIterator forwardIterator) : base(forwardIterator)
        {
        }

        public override IIterator Seek(string key)
        {
            delegateIterator.Seek(key);
            return this;
        }

        public override IIterator SeekToFirst()
        {
            delegateIterator.SeekToLast();
            return this;
        }

        public override IIterator SeekToLast()
        {
            delegateIterator.SeekToFirst();
            return this;
        }

        public override bool MoveNext() => delegateIterator.MovePrevious();
        public override bool MovePrevious() => delegateIterator.MoveNext();

        public override IIterator Reverse() => delegateIterator;
    }
}
