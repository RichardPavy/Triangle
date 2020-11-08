namespace Triangle.LevelDB.Iterators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Triangle.Serialization;
    using Triangle.Utils;
    using Triangle.Visitors;
    using Triangle.Visitors.Utils.Types;

    internal class MergeJoinIterator : AbstractDisposable, IMergeJoinIterator<byte[], byte[], byte[], byte[]>
    {
        private readonly IIterator left;
        private readonly IIterator right;
        private bool isFirstMove = true;

        public MergeJoinIterator(IIterator left, IIterator right)
        {
            this.left = left;
            this.right = right;
        }

        public JoinEntry<byte[], byte[]> Key => JoinEntry.Of(this.left.Key, this.right.Key);
        public JoinEntry<byte[], byte[]> Value => JoinEntry.Of(this.left.Value, this.right.Value);
        public KeyValuePair<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>> Current =>
            new KeyValuePair<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>>(Key, Value);

        object IEnumerator.Current => Current;

        public IIterator<TKey2, TValue2> Cast<TKey2, TValue2>()
        {
            return CastFn<IIterator<TKey2, TValue2>>.Instance(this);
        }

        public IIterator<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>> Cast<TLeftKey, TRightKey, TLeftValue, TRightValue>()
        {
            return Cast<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>>();
        }

        public bool MoveNext()
        {
            if (this.isFirstMove)
            {
                if (!this.left.MoveNext() || !this.right.MoveNext())
                    return false;
                this.isFirstMove = false;
            }

            while (true)
            {
                byte[] leftKey = this.left.Key;
                byte[] rightKey = this.right.Key;
                int length = Math.Min(leftKey.Length, rightKey.Length);
                int comparison = this.left.Key.AsSpan().SequenceCompareTo(this.right.Key.AsSpan().Slice(0, length));
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

        public IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>> Range(
            JoinEntry<byte[], byte[]> from,
            JoinEntry<byte[], byte[]> to)
        {
            return new MergeJoinIterator(
                this.left.Range(from.Left, to.Left),
                this.right.Range(from.Right, to.Right));
        }

        public void Reset()
        {
            this.left.Reset();
            this.right.Reset();
        }

        public IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>> Reverse()
        {
            return new MergeJoinIterator(
                this.left.Reverse(),
                this.right.Reverse());
        }
    }

    internal sealed class MergeJoinIterator<TLeftKey, TRightKey, TLeftValue, TRightValue> : IMergeJoinIterator<TLeftKey, TRightKey, TLeftValue, TRightValue>
    {
        private static readonly Marshaller<TLeftKey> leftKeyMarshaller = Marshallers<TLeftKey>.Instance;
        private static readonly Marshaller<TRightKey> rightKeyMarshaller = Marshallers<TRightKey>.Instance;
        private static readonly Marshaller<TLeftValue> leftValueMarshaller = Marshallers<TLeftValue>.Instance;
        private static readonly Marshaller<TRightValue> rightValueMarshaller = Marshallers<TRightValue>.Instance;

        private readonly IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>> @delegate;

        public MergeJoinIterator(IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>> @delegate)
        {
            this.@delegate = @delegate;
        }

        public JoinEntry<TLeftKey, TRightKey> Key => JoinEntry.Of(
            leftKeyMarshaller.FromBytes(this.@delegate.Current.Key.Left),
            rightKeyMarshaller.FromBytes(this.@delegate.Current.Key.Right));

        public JoinEntry<TLeftValue, TRightValue> Value => JoinEntry.Of(
            leftValueMarshaller.FromBytes(this.@delegate.Current.Key.Left),
            rightValueMarshaller.FromBytes(this.@delegate.Current.Key.Right));

        object IEnumerator.Current => Current;
        public KeyValuePair<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>> Current =>
            new KeyValuePair<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>>(Key, Value);

        public IIterator<TKey2, TValue2> Cast<TKey2, TValue2>() =>
            this.@delegate.Cast<TKey2, TValue2>();
        public IIterator<JoinEntry<TLeftKey2, TRightKey2>, JoinEntry<TLeftValue2, TRightValue2>> Cast<TLeftKey2, TRightKey2, TLeftValue2, TRightValue2>() =>
            this.@delegate.Cast<JoinEntry<TLeftKey2, TRightKey2>, JoinEntry<TLeftValue2, TRightValue2>>();

        public bool MoveNext() => this.@delegate.MoveNext();

        public IIterator<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>> Range(
            JoinEntry<TLeftKey, TRightKey> from,
            JoinEntry<TLeftKey, TRightKey> to) =>
            new MergeJoinIterator<TLeftKey, TRightKey, TLeftValue, TRightValue>(
                this.@delegate.Range(
                    JoinEntry.Of(leftKeyMarshaller.ToBytes(from.Left), rightKeyMarshaller.ToBytes(from.Right)),
                    JoinEntry.Of(leftKeyMarshaller.ToBytes(to.Left), rightKeyMarshaller.ToBytes(to.Right))));

        public IIterator<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>> Reverse() =>
            new MergeJoinIterator<TLeftKey, TRightKey, TLeftValue, TRightValue>(this.@delegate.Reverse());

        public void Reset() => this.@delegate.Reset();
        public void Dispose() => this.@delegate.Dispose();
    }

    internal class CastFn<TOutput> : GenericFunc4<Func<IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>>, TOutput>>
    {
        public static readonly Func<IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>>, TOutput> Instance =
            new CastFn<TOutput>().Call(
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[0].GetGenericArguments()[0],
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[0].GetGenericArguments()[1],
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[1].GetGenericArguments()[0],
                typeof(TOutput).GetGenericParentType(typeof(IIterator<,>)).GetGenericArguments()[1].GetGenericArguments()[1]);

        protected override Func<IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>>, TOutput> Call<TLeftKey, TRightKey, TLeftValue, TRightValue>()
        {
            return (Func<IIterator<JoinEntry<byte[], byte[]>, JoinEntry<byte[], byte[]>>, TOutput>)
                   (Delegate)
                   new Func<IIterator<JoinEntry<byte[],   byte[]>,    JoinEntry<byte[],     byte[]>>,
                            IIterator<JoinEntry<TLeftKey, TRightKey>, JoinEntry<TLeftValue, TRightValue>>>(
                       iterator => new MergeJoinIterator<TLeftKey, TRightKey, TLeftValue, TRightValue>(iterator));
        }
    }
}
