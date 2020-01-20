using System.Text;

namespace LevelDB.Iterators
{
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