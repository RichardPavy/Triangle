namespace LevelDB
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Read options
    /// </summary>
    public class ReadOptions
    {
        /// <summary>
        /// Native handle
        /// </summary>
        internal IntPtr Handle { get; private set; }

        /// <summary>
        /// May be set to true to force checksum verification of all data that
        /// is read from the file system on behalf of a particular read.
        /// By default, no such verification is done.
        /// </summary>
        public bool VerifyChecksums
        {
            set
            {
                leveldb_readoptions_set_verify_checksums(Handle, value);
            }
        }

        /// <summary>
        /// When performing a bulk read, the application may wish to disable
        /// caching so that the data processed by the bulk read does not end up
        /// displacing most of the cached contents.
        /// </summary>
        public bool FillCache
        {
            set
            {
                leveldb_readoptions_set_fill_cache(Handle, value);
            }
        }

        public Snapshot Snapshot
        {
            set
            {
                // keep a reference to Snapshot so it doesn't get GCed
                this.snapshotRef = value;
                if (value == null)
                {
                    leveldb_readoptions_set_snapshot(Handle, IntPtr.Zero);
                }
                else
                {
                    leveldb_readoptions_set_snapshot(Handle, value.Handle);
                }
            }
        }

        private Snapshot snapshotRef;

        public ReadOptions()
        {
            Handle = leveldb_readoptions_create();
        }

        ~ReadOptions()
        {
            leveldb_readoptions_destroy(Handle);
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_readoptions_create();

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_readoptions_destroy(IntPtr readOptions);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_readoptions_set_verify_checksums(IntPtr readOptions, bool value);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_readoptions_set_fill_cache(IntPtr readOptions, bool value);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_readoptions_set_snapshot(IntPtr readOptions, IntPtr snapshot);
    }
}