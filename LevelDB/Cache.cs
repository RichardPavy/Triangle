namespace LevelDB
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// DB cache
    /// </summary>
    public class Cache
    {
        /// <summary>
        /// Native handle
        /// </summary>
        internal IntPtr Handle { get; private set; }

        public int Capacity { get; private set; }

        public Cache(int capacity)
        {
            Capacity = capacity;
            Handle = leveldb_cache_create_lru((UIntPtr)capacity);
        }

        ~Cache()
        {
            leveldb_cache_destroy(Handle);
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_cache_create_lru(UIntPtr capacity);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_cache_destroy(IntPtr cache);
    }
}