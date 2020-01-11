namespace LevelDB.Iterators
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract implementation of iterators.
    /// </summary>
    internal abstract class AbstractIterator : IIterator
    {
        protected bool IsFirstMove { get; set; } = true;

        public abstract string Key { get; }
        public abstract string Value { get; }
        internal abstract bool IsValid { get; }

        object IEnumerator.Current => Current;
        public KeyValuePair<string, string> Current => new KeyValuePair<string, string>(Key, Value);

        public void Reset()
        {
            IsFirstMove = true;
        }

        public virtual void Dispose() { }

        internal abstract void Previous();
        internal abstract void Next();
        internal abstract IIterator SeekToFirst();
        internal abstract IIterator SeekToLast();
        internal abstract IIterator Seek(string key);

        internal virtual int CompareKeys(string a, string b) => a.CompareTo(b);

        public bool MoveNext()
        {
            if (IsFirstMove)
            {
                SeekToFirst();
                IsFirstMove = false;
                return IsValid;
            }
            Next();
            return IsValid;
        }

        public abstract IIterator Reverse();
        public abstract IIterator Range(string from, string to);
    }
}
