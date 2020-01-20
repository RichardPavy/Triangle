namespace LevelDB.Iterators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// DB Iterator
    /// </summary>
    /// <remarks>
    /// This type is not thread safe.
    ///
    /// If two threads share this object, they must protect access to it using
    /// their own locking protocol.
    /// </remarks>
    internal sealed class Iterator : AbstractIterator
    {
        /// <summary>
        /// Native handle
        /// </summary>
        internal IntPtr Handle { get; private set; }

        private DB DB { get; set; }
        private ReadOptions ReadOptions { get; set; }

        internal override bool IsValid => leveldb_iter_valid(Handle) != 0;

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte leveldb_iter_valid(IntPtr iter);

        public override byte[] Key
        {
            get
            {
                UIntPtr keyLength;
                var keyPtr = leveldb_iter_key(Handle, out keyLength);
                return Native.GetBytes(keyPtr, (int)keyLength);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_iter_key(IntPtr iter, out UIntPtr keyLength);

        public override byte[] Value
        {
            get
            {
                UIntPtr valueLength;
                var valuePtr = leveldb_iter_value(Handle, out valueLength);
                return Native.GetBytes(valuePtr, (int)valueLength);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_iter_value(IntPtr iter, out UIntPtr valueLength);

        internal Iterator(DB db, ReadOptions readOptions)
        {
            DB = db;
            // keep reference so it doesn't get GCed
            ReadOptions = readOptions;
            Handle = leveldb_create_iterator(db.Handle, ReadOptions.Handle);
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_create_iterator(IntPtr db, IntPtr readOptions);

        ~Iterator()
        {
            if (DB.Handle != IntPtr.Zero)
            {
                leveldb_iter_destroy(Handle);
            }
        }

        public override IIterator Reverse() => new ReverseIterator(this);
        public override IIterator Range(byte[] from, byte[] to) => new RangeIterator(this, from, to);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_destroy(IntPtr iter);

        internal override IIterator SeekToFirst()
        {
            leveldb_iter_seek_to_first(Handle);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_seek_to_first(IntPtr iter);

        internal override IIterator SeekToLast()
        {
            leveldb_iter_seek_to_last(Handle);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_seek_to_last(IntPtr iter);

        internal override IIterator Seek(byte[] key)
        {
            leveldb_iter_seek(Handle, key, new UIntPtr((uint)key.Length));
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_seek(IntPtr iter, byte[] key, UIntPtr keyLength);

        internal override void Next()
        {
            leveldb_iter_next(Handle);
        }

        internal override void Previous()
        {
            leveldb_iter_prev(Handle);
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_next(IntPtr iter);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_prev(IntPtr iter);
    }

    internal sealed class Iterator<TKey, TValue> : IIterator<TKey, TValue>
    {
        private readonly IIterator delegateIterator;
        private readonly Marshaller<TKey> keyMarshaller = Marshallers<TKey>.Instance;
        private readonly Marshaller<TValue> valueMarshaller = Marshallers<TValue>.Instance;

        internal Iterator(IIterator delegateIterator)
        {
            this.delegateIterator = delegateIterator;
        }

        public TKey Key => keyMarshaller.FromBytes(delegateIterator.Key);
        public TValue Value => valueMarshaller.FromBytes(delegateIterator.Value);
        public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(Key, Value);
        object IEnumerator.Current => Current;
        public void Dispose() => delegateIterator.Dispose();
        public bool MoveNext() => delegateIterator.MoveNext();
        public void Reset() => delegateIterator.Reset();

        public IIterator<TKey, TValue> Range(TKey from, TKey to) =>
            new Iterator<TKey, TValue>(
                delegateIterator.Range(
                    keyMarshaller.ToBytes(from),
                    keyMarshaller.ToBytes(to)));

        public IIterator<TKey, TValue> Reverse() =>
            new Iterator<TKey, TValue>(delegateIterator.Reverse());

        public IIterator<TKey2, TValue2> Cast<TKey2, TValue2>() =>
            new Iterator<TKey2, TValue2>(delegateIterator);
    }
}
