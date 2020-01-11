namespace LevelDB.Iterators
{
    using System.Collections;
    using System.Collections.Generic;

    internal abstract class DelegateIterator : IIterator
    {
        protected readonly IIterator delegateIterator;

        internal DelegateIterator(IIterator delegateIterator)
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

        public virtual IIterator Reverse() => new ReverseIterator(this);
        public virtual IIterator Range(string from, string to) => new RangeIterator(this, from, to);

        public abstract bool MoveNext();
        public abstract bool MovePrevious();
        public abstract IIterator Seek(string key);
        public abstract IIterator SeekToFirst();
        public abstract IIterator SeekToLast();
    }
}
