namespace LevelDB.Iterators
{
    using System;

    /// <summary>
    /// A DB iterator that loops through a range of keys.
    /// </summary>
    internal sealed class RangeIterator : DelegateIterator
    {
        private readonly byte[] from;
        private readonly byte[] to;

        internal override bool IsValid
        {
            get
            {
                if (!base.IsValid)
                {
                    return false;
                }
                byte[] key = Key;
                return CompareKeys(this.from, key) <= 0
                    && CompareKeys(key, this.to) < 0;
            }
        }

        public RangeIterator(AbstractIterator delegateIterator, byte[] from, byte[] to) : base(delegateIterator)
        {
            if (CompareKeys(from, to) > 0)
            {
                throw new ArgumentException($"Invalid range ['{string.Join(",", from)}' - '{string.Join(",", to)}']");
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

        internal override IIterator Seek(byte[] key)
        {
            if (CompareKeys(this.from, key) <= 0 && CompareKeys(key, this.to) <= 0)
            {
                base.Seek(key);
                return this;
            }
            throw new ArgumentOutOfRangeException($"key '{string.Join(",", key)}' is not between ['{string.Join(",", this.from)}' - '{string.Join(",", this.to)}']");
        }

        public override IIterator Reverse() => base.Reverse().Range(this.to, this.from);

        public override IIterator Range(byte[] from, byte[] to)
        {
            if (CompareKeys(this.from, from) <= 0 && CompareKeys(to, this.to) <= 0)
            {
                return base.Range(from, to);
            }
            throw new ArgumentOutOfRangeException($"['{string.Join(",", from)}' - '{string.Join(",", to)}'] is not included in ['{string.Join(",", this.from)}' - '{string.Join(",", this.to)}']");
        }
    }
}
