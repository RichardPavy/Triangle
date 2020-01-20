namespace LevelDB.Iterables
{
    using System.Collections;
    using System.Collections.Generic;
    using LevelDB.Iterators;

    /// <summary>
    /// Abstract implementation of IIterable.
    /// </summary>
    internal abstract class AbstractIterable : IIterable
    {
        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator() => GetIterator();
        IEnumerator IEnumerable.GetEnumerator() => GetIterator();
        public IIterable Range(byte[] from, byte[] to) => new RangeIterable(this, from, to);
        public IIterable Reverse() => new ReverseIterable(this);
        public IIterable<TKey, TValue> Cast<TKey, TValue>() => new Iterable<TKey, TValue>(this);

        public abstract IIterator GetIterator();

        IIterator<byte[], byte[]> IIterable<byte[], byte[]>.GetIterator() => GetIterator();
        IIterable<byte[], byte[]> IIterable<byte[], byte[]>.Reverse() => Reverse();
        IIterable<byte[], byte[]> IIterable<byte[], byte[]>.Range(byte[] from, byte[] to) => Range(from, to);
    }
}
