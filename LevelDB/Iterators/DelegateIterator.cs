namespace LevelDB
{
    using System.Collections;
    using System.Collections.Generic;

    public abstract class DelegateIterator<TSelfIterator, TReverseIterator, TDelegateIterator> : IIterator<TSelfIterator>
        where TSelfIterator : IIterator<TSelfIterator>
        where TReverseIterator : IIterator<TReverseIterator>
        where TDelegateIterator : IIterator<TDelegateIterator>
    {
        protected readonly TDelegateIterator delegateIterator;

        internal DelegateIterator(TDelegateIterator delegateIterator)
        {
            this.delegateIterator = delegateIterator;
        }

        public bool IsValid => delegateIterator.IsValid;
        public string Key => delegateIterator.Key;
        public string Value => delegateIterator.Value;
        public KeyValuePair<string, string> Current => delegateIterator.Current;
        object IEnumerator.Current => delegateIterator.Current;
        public void Reset() => delegateIterator.Reset();
        public void Dispose() => delegateIterator.Dispose();
        IIterator IIterator.Reverse() => Reverse();

        public abstract bool MoveNext();
        public abstract bool MovePrevious();
        public abstract TReverseIterator Reverse();
        public abstract TSelfIterator Seek(string key);
        public abstract TSelfIterator SeekToFirst();
        public abstract TSelfIterator SeekToLast();
    }
}
