namespace Triangle.LevelDB.Iterators
{
    using System;
    using Triangle.Serialization;
    using Triangle.Visitors;
    using Triangle.Visitors.Utils.Types;

    internal class MergeJoinIterator : AbstractMergeJoinIterator<byte[], byte[], byte[], byte[]>
    {
        internal sealed class KeyComparer
        {
            private readonly int sign;
            private readonly int leftPrefixLength;
            private readonly int rightPrefixLength;

            public KeyComparer(int leftPrefixLength, int rightPrefixLength) : this(1, leftPrefixLength, rightPrefixLength)
            {
            }

            private KeyComparer(int sign, int leftPrefixLength, int rightPrefixLength)
            {
                this.sign = sign;
                this.leftPrefixLength = leftPrefixLength;
                this.rightPrefixLength = rightPrefixLength;
            }

            public KeyComparer Reverse()
            {
                return new KeyComparer(-this.sign, this.leftPrefixLength, this.rightPrefixLength);
            }

            public int Compare(byte[] leftKey, byte[] rightKey)
            {
                return this.sign *
                    leftKey.AsSpan().Slice(this.leftPrefixLength).SequenceCompareTo(
                    rightKey.AsSpan().Slice(this.rightPrefixLength, leftKey.Length - this.leftPrefixLength));
            }
        }

        private readonly KeyComparer keyComparer;
        private readonly IIterator<byte[], byte[]> left;
        private readonly IIterator<byte[], byte[]> right;
        private bool isFirstMove = true;

        public MergeJoinIterator(
            KeyComparer keyComparer,
            IIterator<byte[], byte[]> left, IIterator<byte[], byte[]> right)
        {
            this.keyComparer = keyComparer;
            this.left = left;
            this.right = right;
        }

        public override JoinEntry<byte[], byte[]> Key => JoinEntry.Of(this.left.Key, this.right.Key);
        public override JoinEntry<byte[], byte[]> Value => JoinEntry.Of(this.left.Value, this.right.Value);

        public override IMergeJoinIterator<TK1, TK2, TV1, TV2> Cast<TK1, TK2, TV1, TV2>()
            => new MergeJoinIterator<TK1, TK2, TV1, TV2>(this);
        public override IIterator<TKey2, TValue2> Cast<TKey2, TValue2>()
            => CastMergeJoinIteratorFn<IIterator<TKey2, TValue2>>.Instance(this);

        public override bool MoveNext()
        {
            if (this.isFirstMove)
            {
                if (!this.left.MoveNext())
                    return false;
                this.isFirstMove = false;
            }

            if (!this.right.MoveNext())
                return false;

            while (true)
            {
                byte[] leftKey = this.left.Key;
                byte[] rightKey = this.right.Key;
                int comparison = this.keyComparer.Compare(leftKey, rightKey);
                if (comparison == 0)
                {
                    return true;
                }
                else if (comparison < 0)
                {
                    if (!this.left.MoveNext()) return false;
                }
                else
                {
                    if (!this.right.MoveNext()) return false;
                }
            }
        }

        public override void Reset()
        {
            this.left.Reset();
            this.right.Reset();
        }

        public override IMergeJoinIterator<byte[], byte[], byte[], byte[]> Range(
            byte[] fromLeft,
            byte[] fromRight,
            byte[] toLeft,
            byte[] toRight)
            => new MergeJoinIterator(
                this.keyComparer,
                this.left.Range(fromLeft, toLeft),
                this.right.Range(fromRight, toRight));

        public override IMergeJoinIterator<byte[], byte[], byte[], byte[]> Reverse()
            => new MergeJoinIterator(
                this.keyComparer.Reverse(),
                this.left.Reverse(),
                this.right.Reverse());

        protected override void DisposeManagedDependencies()
        {
            this.left.Dispose();
            this.right.Dispose();
        }
    }

    internal sealed class MergeJoinIterator<TK1, TK2, TV1, TV2> : AbstractMergeJoinIterator<TK1, TK2, TV1, TV2>
    {
        private static readonly Marshaller<TK1> leftKeyMarshaller = Marshallers<TK1>.Instance;
        private static readonly Marshaller<TK2> rightKeyMarshaller = Marshallers<TK2>.Instance;
        private static readonly Marshaller<TV1> leftValueMarshaller = Marshallers<TV1>.Instance;
        private static readonly Marshaller<TV2> rightValueMarshaller = Marshallers<TV2>.Instance;

        private readonly MergeJoinIterator @delegate;

        public MergeJoinIterator(MergeJoinIterator @delegate)
        {
            this.@delegate = @delegate;
        }

        public override JoinEntry<TK1, TK2> Key
            => JoinEntry.Of(
                leftKeyMarshaller.FromBytes(this.@delegate.Current.Key.Left),
                rightKeyMarshaller.FromBytes(this.@delegate.Current.Key.Right));

        public override JoinEntry<TV1, TV2> Value
            => JoinEntry.Of(
                leftValueMarshaller.FromBytes(this.@delegate.Current.Value.Left),
                rightValueMarshaller.FromBytes(this.@delegate.Current.Value.Right));

        public override IMergeJoinIterator<TTK1, TTK2, TTV1, TTV2> Cast<TTK1, TTK2, TTV1, TTV2>()
            => this.@delegate.Cast<TTK1, TTK2, TTV1, TTV2>();

        public override IIterator<TKey2, TValue2> Cast<TKey2, TValue2>()
            => this.@delegate.Cast<TKey2, TValue2>();

        public override bool MoveNext() => this.@delegate.MoveNext();

        public override void Reset() => this.@delegate.Reset();

        public override IMergeJoinIterator<TK1, TK2, TV1, TV2> Range(
            TK1 fromLeft,
            TK2 fromRight,
            TK1 toLeft,
            TK2 toRight)
            => this.@delegate
                .Range(
                    leftKeyMarshaller.ToBytes(fromLeft),
                    rightKeyMarshaller.ToBytes(fromRight),
                    leftKeyMarshaller.ToBytes(toLeft),
                    rightKeyMarshaller.ToBytes(toRight))
                .Cast<TK1, TK2, TV1, TV2>();

        public override IMergeJoinIterator<TK1, TK2, TV1, TV2> Reverse()
            => this.@delegate
                .Reverse()
                .Cast<TK1, TK2, TV1, TV2>();

        protected override void DisposeManagedDependencies()
            => this.@delegate.Dispose();
    }

    internal class CastMergeJoinIteratorFn<TOutput> : GenericFunc4<Func<MergeJoinIterator, TOutput>>
    {
        public static readonly Func<MergeJoinIterator, TOutput> Instance =
            new CastMergeJoinIteratorFn<TOutput>().Call(
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[0].GetGenericArguments()[0],
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[0].GetGenericArguments()[1],
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[1].GetGenericArguments()[0],
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[1].GetGenericArguments()[1]);

        protected override Func<MergeJoinIterator, TOutput> Call<TK1, TK2, TV1, TV2>()
        {
            return (Func<MergeJoinIterator, TOutput>)
                   (Delegate)
                   new Func<MergeJoinIterator, IIterator<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>>>(
                       iterator => new MergeJoinIterator<TK1, TK2, TV1, TV2>(iterator));
        }
    }
}
