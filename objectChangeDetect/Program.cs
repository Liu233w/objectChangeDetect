using System;
using Abp.Domain.Entities;
using ChangeTracking;

namespace objectChangeDetect
{
    class Program
    {
        static void Main(string[] args)
        {
            var myEntity = new MyEntity();

            using (var c2Tracker = myEntity.TrackedC2())
            {
                c2Tracker.Obj = new C2
                {
                    C = "c",
                    C1 = new C1
                    {
                        A = "a",
                        B = "b",
                    },
                };
            }

            using (var tracker2 = myEntity.TrackedC2())
            {
                tracker2.Obj.C = "new C";
            }

            Console.WriteLine(myEntity.ExtensionData);
        }
    }

    public class C1
    {
        public virtual string A { get; set; }

        public virtual string B { get; set; }
    }

    public class C2
    {
        public virtual string C { get; set; }

        public virtual C1 C1 { get; set; }
    }

    public class MyEntity : Entity, IExtendableObject
    {
        public string ExtensionData { get; set; }

        public ExtendedObjectTracker<C2> TrackedC2() => this.GetTrackedData<C2>("c2");
    }

    public static class TrackedObjectExtensions
    {
        public static ExtendedObjectTracker<T> GetTrackedData<T>(
            this IExtendableObject extendableObject,
            string name) where T : class
        {
            return new ExtendedObjectTracker<T>(extendableObject, name);
        }
    }

    public class ExtendedObjectTracker<T> : IDisposable
        where T : class
    {
        public T Obj { get; set; }

        private readonly IExtendableObject _entity;

        private readonly string _key;

        public ExtendedObjectTracker(IExtendableObject entity, string key)
        {
            _entity = entity;
            _key = key;

            Obj = entity.GetData<T>(key).AsTrackable();
        }

        public void CheckAndUpdate()
        {
            if (Obj.CastToIChangeTrackable<T>().IsChanged)
            {
                _entity.SetData(_key, Obj);
            }
        }

        public void Dispose()
        {
            CheckAndUpdate();
        }
    }
}
