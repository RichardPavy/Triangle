namespace Triangle.LevelDB.Iterators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Triangle.Utils;

    /// <summary>
    /// Abstract implementation of iterators.
    /// </summary>
    internal abstract class AbstractIterator : AbstractDisposable, IIterator
    {
        private bool isFirstMove = true;

        public abstract byte[] Key { get; }
        public abstract byte[] Value { get; }
        internal abstract bool IsValid { get; }

        object IEnumerator.Current => Current;
        public KeyValuePair<byte[], byte[]> Current => new KeyValuePair<byte[], byte[]>(Key, Value);

        public void Reset()
        {
            this.isFirstMove = true;
        }

        internal abstract void Previous();
        internal abstract void Next();
        internal abstract IIterator SeekToFirst();
        internal abstract IIterator SeekToLast();
        internal abstract IIterator Seek(byte[] key);

        internal virtual int CompareKeys(byte[] a, byte[] b)
        {
            // null is infinity.
            int compareNulls = (a == null).CompareTo(b == null);
            if (compareNulls != 0)
            {
                return compareNulls;
            }
            ReadOnlySpan<byte> aspan = a;
            ReadOnlySpan<byte> bspan = b;
            return aspan.SequenceCompareTo(bspan);
        }

        public bool MoveNext()
        {
            if (this.isFirstMove)
            {
                SeekToFirst();
                this.isFirstMove = false;
                return IsValid;
            }
            Next();
            return IsValid;
        }

        public abstract IIterator Reverse();
        public abstract IIterator Range(byte[] from, byte[] to);

        byte[] IIterator<byte[], byte[]>.Key => Key;
        byte[] IIterator<byte[], byte[]>.Value => Value;
        KeyValuePair<byte[], byte[]> IEnumerator<KeyValuePair<byte[], byte[]>>.Current => Current;
        IIterator<byte[], byte[]> IIterator<byte[], byte[]>.Reverse() => Reverse();
        IIterator<byte[], byte[]> IIterator<byte[], byte[]>.Range(byte[] from, byte[] to) => Range(from, to);

        public IIterator<TKey, TValue> Cast<TKey, TValue>() => new Iterator<TKey, TValue>(this);
    }
}
