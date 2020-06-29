namespace Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    ///   Simple implementation to convert primitives and arrays into binary representation.
    /// </summary>
    /// <remarks>
    ///   Serializer/Deserializer supports more complex data structures.
    /// </remarks>
    /// <seealso cref="Serializer"/>
    /// <seealso cref="Deserializer"/>
    public abstract class Marshallers<T>
    {
        public static readonly Marshaller<T> Instance = Get();

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

    public abstract class Marshaller<T>
    {
        public abstract byte[] ToBytes(T value);
        public abstract T FromBytes(byte[] bytes);
    }

    internal sealed class BytesMarshaller : Marshaller<byte[]>
    {
        public override byte[] FromBytes(byte[] bytes)
        {
            return bytes;
        }

        public override byte[] ToBytes(byte[] value)
        {
            return value;
        }
    }

    internal sealed class StringMarshaller : Marshaller<string>
    {
        public override string FromBytes(byte[] bytes)
        {
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        public override byte[] ToBytes(string value)
        {
            return value == null ? null : Encoding.UTF8.GetBytes(value);
        }
    }

    internal sealed class ValueTypeMarshaller<T> : Marshaller<T>
    {
        private static readonly int Size = Unsafe.SizeOf<T>();

        public override T FromBytes(byte[] data)
        {
            unsafe
            {
                void* dataPointer = Unsafe.AsPointer(ref data[0]);
                return Unsafe.Read<T>(dataPointer);
            }
        }

        public override byte[] ToBytes(T value)
        {
            unsafe
            {
                byte[] data = new byte[Size];
                void* dataPointer = Unsafe.AsPointer(ref data[0]);
                Unsafe.Copy(dataPointer, ref value);
                return data;
            }
        }
    }
}
