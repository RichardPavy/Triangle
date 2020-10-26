namespace Triangle.LevelDB.Iterators
{
    using System.Collections.Generic;

    /// <summary>
    /// DB Iterator interface.
    /// </summary>
    public interface IIterator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        TKey Key { get; }
        TValue Value { get; }
        IIterator<TKey, TValue> Reverse();
        IIterator<TKey, TValue> Range(TKey from, TKey to);
        IIterator<TKey2, TValue2> Cast<TKey2, TValue2>();
    }

    public interface IIterator : IIterator<byte[], byte[]>
    {
        new IIterator Reverse();
        new IIterator Range(byte[] from, byte[] to);
    }
}
