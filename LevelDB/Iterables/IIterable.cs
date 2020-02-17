namespace LevelDB.Iterables
{
    using System.Collections.Generic;
    using LevelDB.Iterators;

    /// <summary>
    /// Represents data in a DB.
    /// </summary>
    public interface IIterable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        IIterator<TKey, TValue> GetIterator();
        IIterable<TKey, TValue> Reverse();
        IIterable<TKey, TValue> Range(TKey from, TKey to);
        IIterable<TKey, TValue> Prefix(TKey prefix);
        IIterable<TKey2, TValue2> Cast<TKey2, TValue2>();

        IIterable<TKey, TValue> Snapshot();
        IIterable<TKey, TValue> FillCache(bool fillCache);
        IIterable<TKey, TValue> VerifyChecksums(bool verifyChecksums);
    }

    public interface IIterable : IIterable<byte[], byte[]>
    {
        new IIterator GetIterator();
        new IIterable Reverse();
        new IIterable Range(byte[] from, byte[] to);
    }
}
