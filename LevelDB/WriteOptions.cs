namespace Triangle.LevelDB
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Write options
    /// </summary>
    public class WriteOptions
    {
        /// <summary>
        /// Native handle
        /// </summary>
        internal IntPtr Handle { get; private set; }

        /// <summary>
        /// This sync flag can be turned on for a particular write to make the
        /// write operation not return until the data being written has been
        /// pushed all the way to persistent storage.
        /// By default, each write to leveldb is asynchronous: it returns after
        /// pushing the write from the process into the operating system. The
        /// transfer from operating system memory to the underlying persistent
        /// storage happens asynchronously.
        /// </summary>
        public bool Sync
        {
            set
            {
                leveldb_writeoptions_set_sync(Handle, value);
            }
        }

        public WriteOptions()
        {
            Handle = leveldb_writeoptions_create();
        }

        ~WriteOptions()
        {
            leveldb_writeoptions_destroy(Handle);
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_writeoptions_create();

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_writeoptions_destroy(IntPtr writeOptions);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_writeoptions_set_sync(IntPtr writeOptions, bool value);
    }
}