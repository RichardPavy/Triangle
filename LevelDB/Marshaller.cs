namespace LevelDB
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class Marshallers<T>
    {
        internal static readonly Marshaller<T> Instance = Get();

        private static Marshaller<T> Get()
        {
            if (typeof(T) == typeof(string))
            {
                return new StringMarshaller() as Marshaller<T>;
            }

            if (typeof(T) == typeof(byte[]))
            {
                return new BytesMarshaller() as Marshaller<T>;
            }

            if (typeof(T).IsValueType)
            {
                return new ValueTypeMarshaller<T>() as Marshaller<T>;
            }

            throw new InvalidOperationException(
                $"Unable to resolve the marshaller for {typeof(T)}");
        }
    }

    internal abstract class Marshaller<T>
    {
        internal abstract byte[] ToBytes(T value);
        internal abstract T FromBytes(byte[] bytes);
    }

    internal sealed class BytesMarshaller : Marshaller<byte[]>
    {
        internal override byte[] FromBytes(byte[] bytes)
        {
            return bytes;
        }

        internal override byte[] ToBytes(byte[] value)
        {
            return value;
        }
    }

    internal sealed class StringMarshaller : Marshaller<string>
    {
        internal override string FromBytes(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        internal override byte[] ToBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
    }

    internal sealed class ValueTypeMarshaller<T> : Marshaller<T>
    {
        internal override T FromBytes(byte[] data)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var value = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return value;
        }

        internal override byte[] ToBytes(T value)
        {
            int size = Marshal.SizeOf<T>();
            byte[] data = new byte[size];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return data;
        }
    }
}
