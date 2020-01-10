namespace LevelDB
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Snapshots provide consistent read-only views over the entire state of
    /// the key-value store. ReadOptions.Snapshot may be non-NULL to indicate
    /// that a read should operate on a particular version of the DB state.
    /// If ReadOptions.Snapshot is NULL, the read will operate on an implicit
    /// snapshot of the current state.
    /// </summary>
    public class Snapshot
    {
        /// <summary>
        /// Native handle
        /// </summary>
        internal IntPtr Handle { get; private set; }

        private DB DB { get; set; }

        internal Snapshot(DB db)
        {
            DB = db;
            Handle = leveldb_create_snapshot(db.Handle);
        }

        ~Snapshot()
        {
            var db = DB.Handle;
            if (db != IntPtr.Zero)
            {
                leveldb_release_snapshot(db, Handle);
            }
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_create_snapshot(IntPtr db);

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern void leveldb_release_snapshot(IntPtr db, IntPtr snapshot);
    }
}