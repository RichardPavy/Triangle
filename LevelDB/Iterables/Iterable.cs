namespace Triangle.LevelDB.Iterables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Triangle.LevelDB.Iterators;
    using Triangle.Serialization;

    /// <summary>
    /// Implementation of IIterable that loops through all the keys.
    /// </summary>
    internal sealed class Iterable : AbstractIterable
    {
        internal override DB DB { get; }

        internal override ReadOptions ReadOptions { get; }

        internal Iterable(DB db, ReadOptions readOptions)
        {
            DB = db;
            ReadOptions = readOptions;
        }

        public override IIterator GetIterator() => new Iterator(DB, ReadOptions);
    }

    internal sealed class Iterable<TKey, TValue> : IIterable<TKey, TValue>
    {
        private readonly IIterable delegateIterable;

        internal Iterable(IIterable delegateIterable)
        {
            if (typeof(TKey) == typeof(byte[]) || typeof(TValue) == typeof(byte[]))
            {
                throw new ArgumentException($"Creating a {nameof(Iterable<TKey, TValue>)}<{typeof(TKey)}, {typeof(TValue)}>");
            }
            this.delegateIterable = delegateIterable;
        }

        public IIterator<TKey, TValue> GetIterator() => this.delegateIterable.GetIterator().Cast<TKey, TValue>();
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => GetIterator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IIterable<TKey, TValue> Range(TKey from, TKey to) =>
            this.delegateIterable
                .Range(
                    Marshallers<TKey>.Instance.ToBytes(from),
                    Marshallers<TKey>.Instance.ToBytes(to))
                .Cast<TKey, TValue>();

        public IIterable<TKey, TValue> Prefix(TKey prefix) =>
            this.delegateIterable
                .Prefix(Marshallers<TKey>.Instance.ToBytes(prefix))
                .Cast<TKey, TValue>();

        public IIterable<TKey, TValue> Reverse() =>
            this.delegateIterable.Reverse().Cast<TKey, TValue>();

        public IIterable<TKey2, TValue2> Cast<TKey2, TValue2>() =>
            typeof(IIterable<TKey2, TValue2>) == typeof(IIterable<byte[], byte[]>)
                ? (IIterable<TKey2, TValue2>) this.delegateIterable
                : new Iterable<TKey2, TValue2>(this.delegateIterable);

        public IIterable<TKey, TValue> Snapshot()
        {
            this.delegateIterable.Snapshot();
            return this;
        }

        public IIterable<TKey, TValue> FillCache(bool fillCache)
        {
            this.delegateIterable.FillCache(fillCache);
            return this;
        }

        public IIterable<TKey, TValue> VerifyChecksums(bool verifyChecksums)
        {
            this.delegateIterable.VerifyChecksums(verifyChecksums);
            return this;
        }
    }
}
