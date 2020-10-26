namespace Triangle.LevelDB
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class Native
    {
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr leveldb_open(IntPtr options, string name, out IntPtr error);

        internal static IntPtr leveldb_open(IntPtr options, string name)
        {
            IntPtr errorPtr;
            var db = leveldb_open(options, name, out errorPtr);
            CheckError(errorPtr);
            return db;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr leveldb_close(IntPtr handle);

        internal static void CheckError(IntPtr errorPtr)
        {
            if (errorPtr == IntPtr.Zero)
            {
                return;
            }
            string errorMessage = GetAndReleaseString(errorPtr);
            if (String.IsNullOrEmpty(errorMessage))
            {
                return;
            }
            throw new ApplicationException(errorMessage);
        }

        private static string GetAndReleaseString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            var str = Marshal.PtrToStringAnsi(ptr);
            leveldb_free(ptr);
            return str;
        }

        internal static byte[] GetBytes(IntPtr ptr, int length)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            byte[] result = new byte[length];
            Marshal.Copy(ptr, result, 0, length);
            return result;
        }

        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void leveldb_free(IntPtr ptr);

        /// <summary>
        /// Returns the major version number for this release.
        /// </summary>
        // extern int leveldb_major_version();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int leveldb_major_version();

        /// <summary>
        /// Returns the minor version number for this release.
        /// </summary>
        // extern int leveldb_minor_version();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int leveldb_minor_version();
    }
}