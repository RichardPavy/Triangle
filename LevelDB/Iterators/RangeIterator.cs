namespace LevelDB.Iterators
{
    using System;

    /// <summary>
    /// A DB iterator that loops through a range of keys.
    /// </summary>
    internal sealed class RangeIterator : DelegateIterator
    {
        private readonly string from;
        private readonly string to;

        internal override bool IsValid
        {
            get
            {
                string key = Key;
                return CompareKeys(this.from, key) <= 0
                    && CompareKeys(key, this.to) < 0
                    && base.IsValid;
            }
        }

        public RangeIterator(AbstractIterator delegateIterator, string from, string to) : base(delegateIterator)
        {
            if (CompareKeys(from, to) > 0)
            {
                throw new ArgumentException($"Invalid range ['{from}' - '{to}']");
            }
            this.from = from;
            this.to = to;
        }

        internal override IIterator SeekToFirst()
        {
            Seek(this.from);
            return this;
        }

        internal override IIterator SeekToLast()
        {
            Seek(this.to);
            return this;
        }

        internal override IIterator Seek(string key)
        {
            if (CompareKeys(this.from, key) <= 0 && CompareKeys(key, this.to) <= 0)
            {
                base.Seek(key);
                return this;
            }
            throw new ArgumentOutOfRangeException($"key '{key}' is not between ['{this.from}' - '{this.to}']");
        }

        public override IIterator Reverse() => base.Reverse().Range(this.to, this.from);

        public override IIterator Range(string from, string to)
        {
            if (CompareKeys(this.from, from) <= 0 && CompareKeys(to, this.to) <= 0)
            {
                return base.Range(from, to);
            }
            throw new ArgumentOutOfRangeException($"['{from}' - '{to}'] is not included in ['{this.from}' - '{this.to}']");
        }
    }
}
