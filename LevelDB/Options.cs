namespace LevelDB
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// DB options
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// If true, the database will be created if it is missing.
        /// Default: false
        /// </summary>
        // bool create_if_missing;
        public bool CreateIfMissing
        {
            set
            {
                leveldb_options_set_create_if_missing(Handle, value);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_create_if_missing(IntPtr options, bool value);

        /// <summary>
        /// If true, an error is raised if the database already exists.
        /// Default: false
        /// </summary>
        // bool error_if_exists;
        public bool ErrorIfExists
        {
            set
            {
                leveldb_options_set_error_if_exists(Handle, value);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_error_if_exists(IntPtr options, bool value);

        /// <summary>
        /// If true, the implementation will do aggressive checking of the
        /// data it is processing and will stop early if it detects any
        /// errors.  This may have unforeseen ramifications: for example, a
        /// corruption of one DB entry may cause a large number of entries to
        /// become unreadable or for the entire DB to become unopenable.
        /// Default: false
        /// </summary>
        // bool paranoid_checks;
        public bool ParanoidChecks
        {
            set
            {
                leveldb_options_set_paranoid_checks(Handle, value);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_paranoid_checks(IntPtr options, bool value);

        /// <summary>
        /// Amount of data to build up in memory (backed by an unsorted log
        /// on disk) before converting to a sorted on-disk file.
        ///
        /// Larger values increase performance, especially during bulk loads.
        /// Up to two write buffers may be held in memory at the same time,
        /// so you may wish to adjust this parameter to control memory usage.
        /// Also, a larger write buffer will result in a longer recovery time
        /// the next time the database is opened.
        ///
        /// Default: 4MB
        /// </summary>
        // size_t write_buffer_size;
        public int WriteBufferSize
        {
            set
            {
                leveldb_options_set_write_buffer_size(Handle, (UIntPtr) value);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_write_buffer_size(IntPtr options, UIntPtr size);

        /// <summary>
        /// Number of open files that can be used by the DB.  You may need to
        /// increase this if your database has a large working set (budget
        /// one open file per 2MB of working set).
        /// Default: 1000
        /// </summary>
        // int max_open_files;
        public int MaxOpenFiles
        {
            set
            {
                leveldb_options_set_max_open_files(Handle, value);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_max_open_files(IntPtr options, int value);

        /// <summary>
        /// Control over blocks (user data is stored in a set of blocks, and
        /// a block is the unit of reading from disk).
        ///
        /// If non-NULL, use the specified cache for blocks.
        /// If NULL, leveldb will automatically create and use an 8MB internal cache.
        /// Default: NULL
        /// </summary>
        // Cache* block_cache;
        public Cache BlockCache
        {
            set
            {
                // keep a reference to Cache so it doesn't get GCed
                this.cacheRef = value;
                if (value == null)
                {
                    leveldb_options_set_cache(Handle, IntPtr.Zero);
                }
                else
                {
                    leveldb_options_set_cache(Handle, value.Handle);
                }
            }
        }

        private Cache cacheRef;

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_cache(IntPtr options, IntPtr cache);

        // extern leveldb_cache_t* leveldb_cache_create_lru(size_t capacity);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_cache_create_lru(UIntPtr capacity);

        // extern void leveldb_cache_destroy(leveldb_cache_t* cache);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_cache_destroy(IntPtr cache);

        /// <summary>
        /// Approximate size of user data packed per block.  Note that the
        /// block size specified here corresponds to uncompressed data.  The
        /// actual size of the unit read from disk may be smaller if
        /// compression is enabled.  This parameter can be changed dynamically.
        ///
        /// Default: 4K
        /// </summary>
        // size_t block_size;
        public int BlockSize
        {
            set
            {
                leveldb_options_set_block_size(Handle, value);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_block_size(IntPtr options, UIntPtr size);
        private static void leveldb_options_set_block_size(IntPtr options, int size)
        {
            leveldb_options_set_block_size(options, (UIntPtr)size);
        }

        /// <summary>
        /// Number of keys between restart points for delta encoding of keys.
        /// This parameter can be changed dynamically.  Most clients should
        /// leave this parameter alone.
        /// Default: 16
        /// </summary>
        // int block_restart_interval;
        public int BlockRestartInterval
        {
            set
            {
                leveldb_options_set_block_restart_interval(Handle, value);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_block_restart_interval(IntPtr options, int interval);

        /// <summary>
        /// Each block is individually compressed before being written to
        /// persistent storage. Compression is on by default since the default
        /// compression method is very fast, and is automatically disabled for
        /// uncompressible data. In rare cases, applications may want to
        /// disable compression entirely, but should only do so if benchmarks
        /// show a performance improvement.
        /// Default: SnappyCompression
        /// </summary>
        // CompressionType compression;
        public CompressionType Compression
        {
            set
            {
                leveldb_options_set_compression(Handle, (int)value);
            }
        }

        public enum CompressionType : int
        {
            NoCompression = 0,
            SnappyCompression = 1
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_set_compression(IntPtr options, int value);

        public Options()
        {
            Handle = leveldb_options_create();
        }

        ~Options()
        {
            leveldb_options_destroy(Handle);
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_options_create();

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_options_destroy(IntPtr options);
    }
}