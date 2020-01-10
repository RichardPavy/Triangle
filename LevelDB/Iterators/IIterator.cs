namespace LevelDB
{
    using System.Collections.Generic;

    /// <summary>
    /// DB Iterator interface.
    /// </summary>
    public interface IIterator<TIterator> : IIterator
        where TIterator : IIterator<TIterator>
    {
        TIterator SeekToFirst();

        TIterator SeekToLast();

        TIterator Seek(string key);
    }

    public interface IIterator : IEnumerator<KeyValuePair<string, string>>
    {
        bool MovePrevious();

        bool IsValid { get; }

        string Key { get; }

        string Value { get; }

        IIterator Reverse();
    }
}
