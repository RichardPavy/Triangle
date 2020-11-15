namespace Triangle.LevelDB.Iterables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Triangle.LevelDB.Iterators;

    /// <summary>
    /// Abstract implementation of IIterable.
    /// </summary>
    internal abstract class AbstractIterable : IIterable
    {
        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator() => GetIterator();
        IEnumerator IEnumerable.GetEnumerator() => GetIterator();
        public IIterable Range(byte[] from, byte[] to) => new RangeIterable(this, from, to);
        public IIterable Reverse() => new ReverseIterable(this);
        public IIterable<TKey, TValue> Cast<TKey, TValue>() =>
            typeof(IIterable<TKey, TValue>) == typeof(IIterable<byte[], byte[]>)
                ? (IIterable<TKey, TValue>) this
                : new Iterable<TKey, TValue>(this);

        public virtual IIterable<byte[], byte[]> Prefix(byte[] prefix)
        {
            byte[] end = null;
            var endLength = prefix.Length;
            for (int i = prefix.Length - 1; i >= 0; i--)
            {
                if (prefix[i] != 255)
                {
                    end = (byte[]) prefix.Clone();
                    end[i]++;
                    break;
                }
                endLength--;
            }

            if (endLength != 0 && endLength != prefix.Length)
            {
                // (endLength != 0) => we hit the break statement => (end != null).
                Array.Resize(ref end, endLength);
            }

            return Range(prefix, end);
        }

        public abstract IIterator GetIterator();

        IIterator<byte[], byte[]> IIterable<byte[], byte[]>.GetIterator() => GetIterator();
        IIterable<byte[], byte[]> IIterable<byte[], byte[]>.Reverse() => Reverse();
        IIterable<byte[], byte[]> IIterable<byte[], byte[]>.Range(byte[] from, byte[] to) => Range(from, to);

        internal abstract DB DB { get; }
        internal abstract ReadOptions ReadOptions { get; }

        public IIterable<byte[], byte[]> Snapshot()
        {
            ReadOptions.Snapshot = DB.CreateSnapshot();
            return this;
        }

        public IIterable<byte[], byte[]> FillCache(bool fillCache)
        {
            ReadOptions.FillCache = fillCache;
            return this;
        }

        public IIterable<byte[], byte[]> VerifyChecksums(bool verifyChecksums)
        {
            ReadOptions.VerifyChecksums = true;
            return this;
        }
    }
}
