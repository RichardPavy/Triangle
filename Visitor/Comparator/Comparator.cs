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
            return CompareVisitorFactory<T>.Visitor.Visit(b, a) != VisitStatus.Exit;
        }

        private static class CompareVisitorFactory<T>
        {
            internal static readonly ClassVisitor<object, T> Visitor = visitorFactory.GetClassVisitor<T>();
        }

        private static readonly VisitorFactory<object> visitorFactory =
            new VisitorFactory<object>(
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

        internal class ObjectComparator : GenericFuncClass<Delegate>
        {
            protected override Delegate Call<TObj>()
            {
                return new ProcessObject<object, TObj>(
                    (object data, TObj a) =>
                    {
                        if (object.ReferenceEquals(a, data))
                        {
                            return VisitStatus.SkipChildren;
                        }
                        if (data is TObj)
                        {
                            return VisitStatus.Continue;
                        }
                        return VisitStatus.Exit;
                    });
            }
        }
        internal class StructComparator : GenericFuncStruct<Delegate>
        {
            protected override Delegate Call<TObj>()
            {
                return new ProcessObject<object, TObj>(
                    (object data, TObj a) =>
                    {
                        if (data is TObj)
                        {
                            return VisitStatus.Continue;
                        }
                        return VisitStatus.Exit;
                    });
            }
        }

        internal class EquatableComparator : GenericFunc<Delegate>
        {
            protected override Delegate Call<TObj>()
            {
                if (!typeof(IEquatable<TObj>).IsAssignableFrom(typeof(TObj)))
                {
                    throw new ArgumentException($"{typeof(TObj)} is not IEquatable<>");
                }
                return new ProcessObject<object, TObj>(
                    (object data, TObj a) =>
                    {
                        return object.Equals(a, data)
                            ? VisitStatus.SkipChildren
                            : VisitStatus.Exit;
                    });
            }
        }

        internal class FieldComparator : GenericFunc2<Func<PropertyInfo, Delegate>>
        {
            protected override Func<PropertyInfo, Delegate> Call<TObj, TValue>()
            {
                return property =>
                {
                    IGetter<TObj, TValue> getter = Getter.Create<TObj, TValue>(property);
                    return new ProcessField<object, TObj, TValue>(
                        (object data, TObj obj, ref TValue valuea) =>
                        {
                            TValue valueb = getter.Apply((TObj)data);
                            VisitorScope<object> result = VisitStatus.Continue;
                            return result + Optional.From<object>(valueb, true);
                        });
                };
            }
        }
    }
}
