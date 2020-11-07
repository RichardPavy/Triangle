namespace Triangle.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    internal abstract class FieldVisitor<TData, TObj> : Visitor
    {
        protected FieldVisitor(MustVisitStatus mustVisit) : base(mustVisit)
        {
        }

        internal abstract VisitStatus Visit(TData data, TObj obj);
    }

    internal abstract class FieldVisitor<TData, TObj, V> : FieldVisitor<TData, TObj>
    {
        protected readonly PropertyInfo property;
        protected readonly ClassVisitor<TData, V> classVisitor;
        private readonly ProcessField<TData, TObj, V> process;

        protected FieldVisitor(
            VisitorFactory<TData> visitorFactory,
            PropertyInfo property,
            ProcessField<TData, TObj, V> process,
            MustVisitStatus mustVisit) : base(mustVisit)
        {
            this.property = property;
            this.classVisitor = visitorFactory.GetClassVisitorImpl<V>();
            this.process = process;
        }

        internal override VisitStatus Visit(TData data, TObj obj)
        {
            V value = Get(obj);

            if (MustVisit != MustVisitStatus.Yes)
                return VisitField(data, value, VisitStatus.Continue);

            VisitorScope<TData> scope = process(data, obj, ref value);
            VisitStatus result = scope.Status == VisitStatus.Continue
                ? VisitField(scope.Data | data, value, scope.Status)
                : scope.Status;
            scope.After?.Invoke();
            return result;
        }

        protected abstract V Get(TObj obj);

        private VisitStatus VisitField(
            TData data,
            V value,
            VisitStatus defaultStatus)
        {
            return classVisitor.Visit(data, value) == VisitStatus.Exit
                ? VisitStatus.Exit
                : defaultStatus;
        }

        protected override IEnumerable<Visitor> ChildVisitors() =>
            ImmutableArray.Create(classVisitor);
    }

    internal sealed class StructFieldVisitor<TData, TObj, V> : FieldVisitor<TData, TObj, V>
    {
        private Getter getter;

        internal StructFieldVisitor(
            VisitorFactory<TData> visitorFactory,
            PropertyInfo property,
            ProcessField<TData, TObj, V> process,
            MustVisitStatus mustVisit) : base(visitorFactory, property, process, mustVisit)
        {
        }

        protected override V Get(TObj obj)
        {
            return getter(obj);
        }

        internal override void Initialize()
        {
            if (this.getter == null)
            {
                this.getter = (Getter) property.GetMethod.CreateDelegate(typeof(Getter));
                this.classVisitor.Initialize();
            }
        }

        private delegate V Getter(in TObj obj);
    }

    internal sealed class ClassFieldVisitor<TData, TObj, V> : FieldVisitor<TData, TObj, V>
    {
        private Getter getter;

        internal ClassFieldVisitor(
            VisitorFactory<TData> visitorFactory,
            PropertyInfo property,
            ProcessField<TData, TObj, V> process,
            MustVisitStatus mustVisit) : base(visitorFactory, property, process, mustVisit)
        {
        }

        protected override V Get(TObj obj)
        {
            return getter(obj);
        }

        internal override void Initialize()
        {
            if (this.getter == null)
            {
                Console.WriteLine(property);
                try
                {
                    this.getter = (Getter)property.GetMethod.CreateDelegate(typeof(Getter));
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Property {property.Name}\n\treturning {property.PropertyType}\n\tin type {property.DeclaringType}\n\tdoes not match {typeof(Getter)}",
                        exception);
                }
                this.classVisitor.Initialize();
            }
        }

        private delegate V Getter(TObj obj);
    }
}
