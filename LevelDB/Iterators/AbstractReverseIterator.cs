namespace LevelDB
{
    /// <summary>
    /// Abstract implementation of reverse iterators.
    /// </summary>
    public abstract class AbstractReverseIterator<TSelfIterator, TForwardIterator> : DelegateIterator<TSelfIterator, TForwardIterator, TForwardIterator>
        where TSelfIterator : IIterator<TSelfIterator>
        where TForwardIterator : IIterator<TForwardIterator>
    {
        internal AbstractReverseIterator(TForwardIterator forwardIterator) : base(forwardIterator)
        {
        }

        protected abstract TSelfIterator Self { get; }

        public override bool MoveNext() => delegateIterator.MovePrevious();
        public override bool MovePrevious() => delegateIterator.MoveNext();

        public override TForwardIterator Reverse()
        {
            return delegateIterator;
        }

        public override TSelfIterator Seek(string key)
        {
            delegateIterator.Seek(key);
            return Self;
        }

        public override TSelfIterator SeekToFirst()
        {
            delegateIterator.SeekToLast();
            return Self;
        }

        public override TSelfIterator SeekToLast()
        {
            delegateIterator.SeekToFirst();
            return Self;
        }
    }
}
