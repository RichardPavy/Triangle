namespace LevelDB
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a range of keys.
    /// </summary>
    public class Range : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly DB db;
        private readonly string from;
        private readonly string to;

        internal Range(DB db, string from, string to)
        {
            this.db = db;
            this.from = from;
            this.to = to;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return new RangeIterator<Iterator>(db.GetIterator(), this.from, this.to);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
