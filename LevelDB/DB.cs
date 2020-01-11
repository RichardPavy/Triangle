namespace LevelDB
{
    using System;
    using System.Runtime.InteropServices;
    using LevelDB.Iterables;

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
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;

            this.options = null;
            if (Handle != IntPtr.Zero)
            {
                Native.leveldb_close(Handle);
                Handle = IntPtr.Zero;
            }
        }

        public string this[string key]
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

        public DB Put(string key, string value)
        {
            return Put(new WriteOptions(), key, value);
        }

        public DB Put(WriteOptions writeOptions, string key, string value)
        {
            CheckDisposed();
            IntPtr errorPtr;
            var keyLength = Native.GetStringLength(key);
            var valueLength = Native.GetStringLength(value);
            leveldb_put(
                Handle, writeOptions.Handle,
                key, keyLength,
                value, valueLength,
                out errorPtr);
            Native.CheckError(errorPtr);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_put(
            IntPtr handle,
            IntPtr writeOptions,
            string key,
            UIntPtr keyLength,
            string value,
            UIntPtr valueLength,
            out IntPtr errorPtr);


        public DB Delete(string key)
        {
            return Delete(new WriteOptions(), key);
        }

        public DB Delete(WriteOptions writeOptions, string key)
        {
            CheckDisposed();
            IntPtr errorPtr;
            var keyLength = Native.GetStringLength(key);
            leveldb_delete(Handle, writeOptions.Handle, key, keyLength, out errorPtr);
            Native.CheckError(errorPtr);
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_delete(
            IntPtr handle,
            IntPtr writeOptions,
            string key,
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

        public string Get(string key)
        {
            return Get(new ReadOptions(), key);
        }

        public string Get(ReadOptions readOptions, string key)
        {
            CheckDisposed();
            UIntPtr valueLength;
            IntPtr errorPtr;
            var keyLength = Native.GetStringLength(key);
            var valuePtr = leveldb_get(
                Handle, readOptions.Handle,
                key, keyLength,
                out valueLength,
                out errorPtr);
            Native.CheckError(errorPtr);
            if (valuePtr == IntPtr.Zero || valueLength == UIntPtr.Zero)
            {
                return null;
            }
            var value = Marshal.PtrToStringAnsi(valuePtr, (int)valueLength);
            Native.leveldb_free(valuePtr);
            return value;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_get(
            IntPtr handle,
            IntPtr readOptions,
            string key,
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
        public DB CompactRange(string startKey, string limitKey)
        {
            CheckDisposed();
            leveldb_compact_range(
                Handle,
                startKey, Native.GetStringLength(startKey),
                limitKey, Native.GetStringLength(limitKey));
            return this;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_compact_range(
            IntPtr handle,
            string startKey, UIntPtr startKeyLen,
            string limitKey, UIntPtr limitKeyLen);

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
            if (isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}