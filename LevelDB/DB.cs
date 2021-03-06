namespace Triangle.LevelDB
{
    using System;
    using System.Runtime.InteropServices;
    using Triangle.LevelDB.Iterables;
    using Triangle.Serialization;

    public class DB : IDisposable
    {

        /// <summary>
        /// Native handle
        /// </summary>
        internal IntPtr Handle { get; private set; }

        private Options options;
        private bool isDisposed;

        public DB(Options options, string path)
        {
            // keep a reference to options as it might contain a cache object
            // which needs to stay alive as long as the DB is not closed
            this.options = options;
            Handle = Native.leveldb_open(options.Handle, path);
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }
            this.isDisposed = true;

            this.options = null;
            if (Handle != IntPtr.Zero)
            {
                Native.leveldb_close(Handle);
                Handle = IntPtr.Zero;
            }
        }

        public byte[] this[byte[] key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Put(key, value);
            }
        }

        public DB Put(byte[] key, byte[] value)
        {
            return Put(new WriteOptions(), key, value);
        }

        public DB Put(WriteOptions writeOptions, byte[] key, byte[] value)
        {
            CheckDisposed();
            IntPtr errorPtr;
            leveldb_put(
                Handle, writeOptions.Handle,
                key, new UIntPtr((uint)key.Length),
                value, new UIntPtr((uint)value.Length),
                out errorPtr);
            Native.CheckError(errorPtr);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_put(
            IntPtr handle,
            IntPtr writeOptions,
            byte[] key,
            UIntPtr keyLength,
            byte[] value,
            UIntPtr valueLength,
            out IntPtr errorPtr);


        public DB Delete(byte[] key)
        {
            return Delete(new WriteOptions(), key);
        }

        public DB Delete(WriteOptions writeOptions, byte[] key)
        {
            CheckDisposed();
            IntPtr errorPtr;
            leveldb_delete(Handle, writeOptions.Handle, key, new UIntPtr((uint)key.Length), out errorPtr);
            Native.CheckError(errorPtr);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_delete(
            IntPtr handle,
            IntPtr writeOptions,
            byte[] key,
            UIntPtr keylen,
            out IntPtr errorPtr);

        /*
                public void Write(WriteOptions writeOptions, WriteBatch writeBatch)
                {
                    CheckDisposed();
                    if (writeOptions == null)
                    {
                        writeOptions = new WriteOptions();
                    }
                    if (writeBatch == null)
                    {
                        throw new ArgumentNullException("writeBatch");
                    }
                    Native.leveldb_write(Handle, writeOptions.Handle, writeBatch.Handle);
                }

                public void Write(WriteBatch writeBatch)
                {
                    Write(null, writeBatch);
                }
        */

        public byte[] Get(byte[] key)
        {
            return Get(new ReadOptions(), key);
        }

        public byte[] Get(ReadOptions readOptions, byte[] key)
        {
            CheckDisposed();
            UIntPtr valueLength;
            IntPtr errorPtr;
            var valuePtr = leveldb_get(
                Handle, readOptions.Handle,
                key, new UIntPtr((uint)key.Length),
                out valueLength,
                out errorPtr);
            Native.CheckError(errorPtr);
            if (valuePtr == IntPtr.Zero || valueLength == UIntPtr.Zero)
            {
                return null;
            }
            var value = Native.GetBytes(valuePtr, (int)valueLength);
            Native.leveldb_free(valuePtr);
            return value;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_get(
            IntPtr handle,
            IntPtr readOptions,
            byte[] key,
            UIntPtr keyLength,
            out UIntPtr valueLength,
            out IntPtr errorPtr);

        public IIterable GetIterable()
        {
            return GetIterable(new ReadOptions());
        }

        public IIterable GetIterable(ReadOptions readOptions)
        {
            CheckDisposed();
            return new Iterable(this, readOptions);
        }

        public Snapshot CreateSnapshot()
        {
            CheckDisposed();
            return new Snapshot(this);
        }

        /// <summary>
        /// Compacts the entire database.
        /// </summary>
        /// <seealso cref="DB.CompactRange"/>
        public DB Compact()
        {
            return CompactRange(null, null);
        }

        /// <summary>
        /// Compact the underlying storage for the key range [startKey,limitKey].
        /// In particular, deleted and overwritten versions are discarded,
        /// and the data is rearranged to reduce the cost of operations
        /// needed to access the data.  This operation should typically only
        /// be invoked by users who understand the underlying implementation.
        /// </summary>
        /// <remarks>
        /// CompactRange(null, null) will compact the entire database
        /// </remarks>
        /// <param name="startKey">
        /// null is treated as a key before all keys in the database
        /// </param>
        /// <param name="limitKey">
        /// null is treated as a key after all keys in the database
        /// </param>
        public DB CompactRange(byte[] startKey, byte[] limitKey)
        {
            CheckDisposed();
            leveldb_compact_range(
                Handle,
                startKey, new UIntPtr((uint)startKey.Length),
                limitKey, new UIntPtr((uint)limitKey.Length));
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_compact_range(
            IntPtr handle,
            byte[] startKey, UIntPtr startKeyLen,
            byte[] limitKey, UIntPtr limitKeyLen);

        /// <summary>
        /// DB implementations can export properties about their state via this
        /// method.  If "property" is a valid property understood by this DB
        /// implementation, it returns its current value. Otherwise it returns
        /// null.
        ///
        /// Valid property names include:
        ///  "leveldb.num-files-at-level<N>" - return the number of files at level <N>,
        ///     where <N> is an ASCII representation of a level number (e.g. "0").
        ///  "leveldb.stats" - returns a multi-line string that describes statistics
        ///     about the internal operation of the DB.
        ///  "leveldb.sstables" - returns a multi-line string that describes all
        ///     of the sstables that make up the db contents.
        /// </summary>
        public string GetProperty(string property)
        {
            CheckDisposed();
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            var valuePtr = leveldb_property_value(Handle, property);
            if (valuePtr == IntPtr.Zero)
            {
                return null;
            }
            var value = Marshal.PtrToStringAnsi(valuePtr);
            Native.leveldb_free(valuePtr);
            return value;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_property_value(IntPtr db, string propname);

        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        public DB<TKey, TValue> Cast<TKey, TValue>() => new DB<TKey, TValue>(this);
    }

    public class DB<TKey, TValue> : IDisposable
    {
        private readonly DB db;
        private readonly Marshaller<TKey> keyMarshaller = Marshallers<TKey>.Instance;
        private readonly Marshaller<TValue> valueMarshaller = Marshallers<TValue>.Instance;

        internal DB(DB db)
        {
            this.db = db;
        }

        public void Dispose() => this.db.Dispose();

        public TValue this[TKey key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Put(key, value);
            }
        }

        public DB<TKey, TValue> Put(TKey key, TValue value)
        {
            this.db.Put(this.keyMarshaller.ToBytes(key), this.valueMarshaller.ToBytes(value));
            return this;
        }

        public DB<TKey, TValue> Put(WriteOptions writeOptions, TKey key, TValue value)
        {
            this.db.Put(writeOptions, this.keyMarshaller.ToBytes(key), this.valueMarshaller.ToBytes(value));
            return this;
        }

        public DB<TKey, TValue> Delete(TKey key)
        {
            this.db.Delete(this.keyMarshaller.ToBytes(key));
            return this;
        }

        public DB<TKey, TValue> Delete(WriteOptions writeOptions, TKey key)
        {
            this.db.Delete(writeOptions, this.keyMarshaller.ToBytes(key));
            return this;
        }

        /*
        public void Write(WriteOptions writeOptions, WriteBatch writeBatch)
        {
        }

        public void Write(WriteBatch writeBatch)
        {
        }
        */

        public TValue Get(TKey key)
        {
            return this.valueMarshaller.FromBytes(this.db.Get(this.keyMarshaller.ToBytes(key)));
        }

        public TValue Get(ReadOptions readOptions, TKey key)
        {
            return this.valueMarshaller.FromBytes(this.db.Get(readOptions, this.keyMarshaller.ToBytes(key)));
        }

        public IIterable<TKey, TValue> GetIterable()
        {
            return this.db.GetIterable().Cast<TKey, TValue>();
        }

        public IIterable<TKey, TValue> GetIterable(ReadOptions readOptions)
        {
            return this.db.GetIterable(readOptions).Cast<TKey, TValue>();
        }

        public Snapshot CreateSnapshot()
        {
            return this.db.CreateSnapshot();
        }

        public DB<TKey, TValue> Compact()
        {
            this.db.Compact();
            return this;
        }

        public DB<TKey, TValue> CompactRange(TKey startKey, TKey limitKey)
        {
            this.db.CompactRange(this.keyMarshaller.ToBytes(startKey), this.keyMarshaller.ToBytes(limitKey));
            return this;
        }

        public string GetProperty(string property)
        {
            return this.db.GetProperty(property);
        }
        
        public DB<TKey2, TValue2> Cast<TKey2, TValue2>() => new DB<TKey2, TValue2>(this.db);
   }
}