namespace LevelDB
{
    using System;
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
                return new NoopMarshaller() as Marshaller<T>;
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

    internal sealed class NoopMarshaller : Marshaller<byte[]>
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
}