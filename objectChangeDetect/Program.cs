using System;
using Abp.Domain.Entities;

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

    class C1
    {
        public string A { get; set; }

        public string B { get; set; }
    }

    class C2
    {
        public string C { get; set; }

        public C1 C1 { get; set; }
    }

    class MyEntity : Entity, IExtendableObject
    {
        public string ExtensionData { get; set; }

        public ExtendedObjectTracker<C2> TrackedC2() => this.GetTrackedData<C2>("c2");
    }

    static class TrackedObjectExtensions
    {
        public static ExtendedObjectTracker<T> GetTrackedData<T>(
            this IExtendableObject extendableObject,
            string name)
        {
            return new ExtendedObjectTracker<T>(extendableObject, name);
        }
    }

    class ExtendedObjectTracker<T> : IDisposable
    {
        public T Obj { get; set; }

        private readonly IExtendableObject _entity;

        private readonly string _key;

        private readonly int _beginHash;

        public ExtendedObjectTracker(IExtendableObject entity, string key)
        {
            _entity = entity;
            _key = key;

            Obj = entity.GetData<T>(key);
            _beginHash = Obj == null ? 0 : Obj.GetHashCode();
        }

        public void CheckAndUpdate()
        {
            var endHash = Obj == null ? 0 : Obj.GetHashCode();
            if (endHash != _beginHash)
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
