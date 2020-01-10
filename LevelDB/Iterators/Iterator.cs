namespace LevelDB
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
    public sealed class Iterator : IIterator<Iterator>
    {
        /// <summary>
        /// Native handle
        /// </summary>
        internal IntPtr Handle { get; private set; }

        private DB DB { get; set; }
        private ReadOptions ReadOptions { get; set; }
        private bool IsFirstMove { get; set; }

        public bool IsValid => leveldb_iter_valid(Handle) != 0;

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern byte leveldb_iter_valid(IntPtr iter);

        public string Key
        {
            get
            {
                UIntPtr keyLength;
                var keyPtr = leveldb_iter_key(Handle, out keyLength);
                if (keyPtr == IntPtr.Zero || keyLength == UIntPtr.Zero)
                {
                    return null;
                }
                var key = Marshal.PtrToStringAnsi(keyPtr, (int)keyLength);
                return key;
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_iter_key(IntPtr iter, out UIntPtr keyLength);

        public string Value
        {
            get
            {
                UIntPtr valueLength;
                var valuePtr = leveldb_iter_value(Handle, out valueLength);
                if (valuePtr == IntPtr.Zero || valueLength == UIntPtr.Zero)
                {
                    return null;
                }
                var value = Marshal.PtrToStringAnsi(valuePtr, (int)valueLength);
                return value;
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_iter_value(IntPtr iter, out UIntPtr valueLength);

        object IEnumerator.Current => Current;
        public KeyValuePair<string, string> Current => new KeyValuePair<string, string>(Key, Value);

        internal Iterator(DB db, ReadOptions readOptions)
        {
            DB = db;
            // keep reference so it doesn't get GCed
            ReadOptions = readOptions;
            Handle = leveldb_create_iterator(db.Handle, ReadOptions.Handle);
            IsFirstMove = true;
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

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_destroy(IntPtr iter);

        public Iterator SeekToFirst()
        {
            leveldb_iter_seek_to_first(Handle);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_seek_to_first(IntPtr iter);

        public Iterator SeekToLast()
        {
            leveldb_iter_seek_to_last(Handle);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_seek_to_last(IntPtr iter);

        public Iterator Seek(string key)
        {
            leveldb_iter_seek(Handle, key, Native.GetStringLength(key));
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_seek(IntPtr iter, string key, UIntPtr keyLength);

        public void Reset()
        {
            IsFirstMove = true;
        }

        public bool MoveNext()
        {
            if (IsFirstMove)
            {
                SeekToFirst();
                IsFirstMove = false;
                return IsValid;
            }
            leveldb_iter_next(Handle);
            return IsValid;
        }

        public bool MovePrevious()
        {
            if (IsFirstMove)
            {
                SeekToLast();
                IsFirstMove = false;
                return IsValid;
            }
            leveldb_iter_prev(Handle);
            return IsValid;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_next(IntPtr iter);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_iter_prev(IntPtr iter);

        public ReverseIterator Reverse() => new ReverseIterator(this);
        IIterator IIterator.Reverse() => Reverse();

        public void Dispose()
        {
            // ~Iterator() takes already care
        }
    }
}
