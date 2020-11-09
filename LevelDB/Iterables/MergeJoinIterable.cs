namespace Triangle.LevelDB.Iterables
{
    using System;
    using Triangle.LevelDB.Iterators;
    using Triangle.Serialization;
    using Triangle.Visitors;
    using Triangle.Visitors.Utils.Types;

    public sealed class MergeJoinIterable : AbstractMergeJoinIterable<byte[], byte[], byte[], byte[]>
    {
        private readonly IIterable left;
        private readonly IIterable right;

        internal MergeJoinIterable(IIterable left, IIterable right)
        {
            this.left = left;
            this.right = right;
        }

        public override IMergeJoinIterator<byte[], byte[], byte[], byte[]> GetIterator()
            => new MergeJoinIterator(
                this.left.GetIterator(),
                this.right.GetIterator());

        public override IMergeJoinIterable<byte[], byte[], byte[], byte[]> Reverse()
            => new MergeJoinIterable(
                this.left.Reverse(),
                this.right.Reverse());

        public override IMergeJoinIterable<byte[], byte[], byte[], byte[]> Range(
            byte[] fromLeft,
            byte[] fromRight,
            byte[] toLeft,
            byte[] toRight)
            => new MergeJoinIterable(
                this.left.Range(fromLeft, toLeft),
                this.right.Range(fromRight, toRight));

        public override IMergeJoinIterable<byte[], byte[], byte[], byte[]> Prefix(
            byte[] prefixLeft,
            byte[] prefixRight)
            => new MergeJoinIterable(
                this.left.Prefix(prefixLeft),
                this.right.Prefix(prefixRight));

        public override IMergeJoinIterable<TK1, TK2, TV1, TV2> Cast<TK1, TK2, TV1, TV2>()
            => new MergeJoinIterable<TK1, TK2, TV1, TV2>(this);

        public override IIterable<TKey2, TValue2> Cast<TKey2, TValue2>()
            => CastMergeJoinIterableFn<IIterable<TKey2, TValue2>>.Instance(this);

        public override IMergeJoinIterable<byte[], byte[], byte[], byte[]> Snapshot()
        {
            this.left.Snapshot();
            this.right.Snapshot();
            return this;
        }

        public override IMergeJoinIterable<byte[], byte[], byte[], byte[]> FillCache(bool fillCache)
        {
            this.left.FillCache(fillCache);
            this.right.FillCache(fillCache);
            return this;
        }

        public override IMergeJoinIterable<byte[], byte[], byte[], byte[]> VerifyChecksums(bool verifyChecksums)
        {
            this.left.VerifyChecksums(verifyChecksums);
            this.right.VerifyChecksums(verifyChecksums);
            return this;
        }
    }

    public sealed class MergeJoinIterable<TK1, TK2, TV1, TV2> : AbstractMergeJoinIterable<TK1, TK2, TV1, TV2>
    {
        private static readonly Marshaller<TK1> leftKeyMarshaller = Marshallers<TK1>.Instance;
        private static readonly Marshaller<TK2> rightKeyMarshaller = Marshallers<TK2>.Instance;

        private readonly IMergeJoinIterable<byte[], byte[], byte[], byte[]> @delegate;

        public MergeJoinIterable(MergeJoinIterable @delegate)
        {
            this.@delegate = @delegate;
        }

        public override IMergeJoinIterator<TK1, TK2, TV1, TV2> GetIterator()
        {
            return this.@delegate.GetIterator().Cast<TK1, TK2, TV1, TV2>();
        }

        public override IMergeJoinIterable<TK1, TK2, TV1, TV2> Reverse()
        {
            return this.@delegate.Reverse().Cast<TK1, TK2, TV1, TV2>();
        }

        public override IMergeJoinIterable<TK1, TK2, TV1, TV2> Range(
            TK1 fromLeft,
            TK2 fromRight,
            TK1 toLeft,
            TK2 toRight)
        {
            return this.@delegate
                .Range(
                    fromLeft: leftKeyMarshaller.ToBytes(fromLeft),
                    fromRight: rightKeyMarshaller.ToBytes(fromRight),
                    toLeft: leftKeyMarshaller.ToBytes(toLeft),
                    toRight: rightKeyMarshaller.ToBytes(toRight))
                .Cast<TK1, TK2, TV1, TV2>();
        }

        public override IMergeJoinIterable<TK1, TK2, TV1, TV2> Prefix(
            TK1 prefixLeft,
            TK2 prefixRight)
        {
            return this.@delegate
                .Prefix(
                    prefixLeft: leftKeyMarshaller.ToBytes(prefixLeft),
                    prefixRight: rightKeyMarshaller.ToBytes(prefixRight))
                .Cast<TK1, TK2, TV1, TV2>();
        }

        public override IMergeJoinIterable<TTK1, TTK2, TTV1, TTV2> Cast<TTK1, TTK2, TTV1, TTV2>()
            => this.@delegate.Cast<TTK1, TTK2, TTV1, TTV2>();
        public override IIterable<TKey2, TValue2> Cast<TKey2, TValue2>()
            => this.@delegate.Cast<TKey2, TValue2>();

        public override IMergeJoinIterable<TK1, TK2, TV1, TV2> Snapshot()
        {
            this.@delegate.Snapshot();
            return this;
        }

        public override IMergeJoinIterable<TK1, TK2, TV1, TV2> FillCache(bool fillCache)
        {
            this.@delegate.FillCache(fillCache);
            return this;
        }

        public override IMergeJoinIterable<TK1, TK2, TV1, TV2> VerifyChecksums(bool verifyChecksums)
        {
            this.@delegate.VerifyChecksums(verifyChecksums);
            return this;
        }
    }

    internal class CastMergeJoinIterableFn<TOutput> : GenericFunc4<Func<MergeJoinIterable, TOutput>>
    {
        public static readonly Func<MergeJoinIterable, TOutput> Instance =
            new CastMergeJoinIterableFn<TOutput>().Call(
                typeof(TOutput).GetGenericParentType(typeof(IIterable<,>)).GetGenericArguments()[0].GetGenericArguments()[0],
                typeof(TOutput).GetGenericParentType(typeof(IIterable<,>)).GetGenericArguments()[0].GetGenericArguments()[1],
                typeof(TOutput).GetGenericParentType(typeof(IIterable<,>)).GetGenericArguments()[1].GetGenericArguments()[0],
                typeof(TOutput).GetGenericParentType(typeof(IIterable<,>)).GetGenericArguments()[1].GetGenericArguments()[1]);

        protected override Func<MergeJoinIterable, TOutput> Call<TK1, TK2, TV1, TV2>()
        {
            return (Func<MergeJoinIterable, TOutput>)
                   (Delegate)
                   new Func<MergeJoinIterable, IIterable<JoinEntry<TK1, TK2>, JoinEntry<TV1, TV2>>>(
                       iterable => new MergeJoinIterable<TK1, TK2, TV1, TV2>(iterable));
        }
    }
}
