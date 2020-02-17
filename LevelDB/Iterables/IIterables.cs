namespace LevelDB.Iterables
{
    using System;
    using System.Collections.Generic;

    public static class Iterables
    {
        public static IEnumerable<KeyValuePair<Tuple<TK1, TK2>, Tuple<TV1, TV2>>> Join<TK1, TV1, TK2, TV2>(
            this IIterable<TK1, TV1> e1,
            IIterable<TK2, TV2> e2,
            Func<TK1, TK2, int> compare)
        {
            using var it1 = e1.GetIterator();
            using var it2 = e2.GetIterator();

            if (!it1.MoveNext() || !it2.MoveNext())
            {
                yield break;
            }

            KeyValuePair<TK1, TV1> kv1 = it1.Current;
            KeyValuePair<TK2, TV2> kv2 = it2.Current;

            while (true)
            {
                int comparison = compare(kv1.Key, kv2.Key);
                if (comparison == 0)
                {
                    // kv1 == kv2
                    yield return new KeyValuePair<Tuple<TK1, TK2>, Tuple<TV1, TV2>>(
                        Tuple.Create(kv1.Key, kv2.Key),
                        Tuple.Create(kv1.Value, kv2.Value));

                    if (!it2.MoveNext()) yield break;
                    kv2 = it2.Current;
                }
                else if (comparison < 0)
                {
                    // kv1 < kv2
                    if (!it1.MoveNext()) yield break;
                    kv1 = it1.Current;
                }
                else if (comparison > 0)
                {
                    // kv1 > kv2
                    if (!it2.MoveNext()) yield break;
                    kv2 = it2.Current;
                }
            }
        }
    }
}
