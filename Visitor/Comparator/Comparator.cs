namespace Visitors.Comparators
{
    using System;
    using System.Reflection;
    using Visitors.Utils;
    using Visitors.Utils.Types;

    public sealed class Comparator
    {
        public static bool Compare<T>(T a, T b)
        {
            return CompareVisitorFactory<T>.Visitor.Visit(new Data(b), a) != VisitStatus.Exit;
        }

        private static class CompareVisitorFactory<T>
        {
            internal static readonly ClassVisitor<Data, T> Visitor = visitorFactory.GetClassVisitor<T>();
        }

        private static readonly VisitorFactory<Data> visitorFactory =
            new VisitorFactory<Data>(
                type =>
                {
                    if (type.GetGenericParentType(typeof(IEquatable<>)) != null)
                    {
                        return new EquatableComparator().Call(type);
                    }
                    if (type.IsValueType)
                    {
                        return new StructComparator().Call(type);
                    }
                    return new ObjectComparator().Call(type);
                },
                property =>
                {
                    return new FieldComparator().Call(property)(property);
                });

        private class ObjectComparator : GenericFuncClass<Delegate>
        {
            protected override Delegate Call<TObj>()
            {
                return new ProcessObject<Data, TObj>(
                    (Data data, TObj a) =>
                    {
                        if (object.ReferenceEquals(a, data.X))
                        {
                            return VisitStatus.SkipChildren;
                        }
                        if (data.X is TObj)
                        {
                            return VisitStatus.Continue;
                        }
                        return VisitStatus.Exit;
                    });
            }
        }

        private class StructComparator : GenericFuncStruct<Delegate>
        {
            protected override Delegate Call<TObj>()
            {
                return new ProcessObject<Data, TObj>(
                    (Data data, TObj a) =>
                    {
                        if (data.X is TObj)
                        {
                            return VisitStatus.Continue;
                        }
                        return VisitStatus.Exit;
                    });
            }
        }

        private class EquatableComparator : GenericFunc<Delegate>
        {
            protected override Delegate Call<TObj>()
            {
                if (!typeof(IEquatable<TObj>).IsAssignableFrom(typeof(TObj)))
                {
                    throw new ArgumentException($"{typeof(TObj)} is not IEquatable<>");
                }
                return new ProcessObject<Data, TObj>(
                    (Data data, TObj a) =>
                    {
                        return object.Equals(a, data.X)
                            ? VisitStatus.SkipChildren
                            : VisitStatus.Exit;
                    });
            }
        }

        private class FieldComparator : GenericFunc2<Func<PropertyInfo, Delegate>>
        {
            protected override Func<PropertyInfo, Delegate> Call<TObj, TValue>()
            {
                return property =>
                {
                    IGetter<TObj, TValue> getter = Getter.Create<TObj, TValue>(property);
                    return new ProcessField<Data, TObj, TValue>(
                        (Data data, TObj obj, ref TValue valuea) =>
                        {
                            TValue valueb = getter.Apply((TObj) data.X);
                            VisitorScope<Data> result = VisitStatus.Continue;
                            return result + Optional.From(new Data(valueb));
                        });
                };
            }
        }

        private struct Data
        {
            internal readonly object X;

            internal Data(object x)
            {
                this.X = x;
            }
        }
    }
}
