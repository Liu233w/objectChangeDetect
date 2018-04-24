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

            myEntity.C2 = new C2
            {
                C = "c",
                C1 = new C1
                {
                    A = "a",
                    B = "b",
                },
            };

            myEntity.C2.C = "new C";
            myEntity.C2.C1.A = "new A";

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

        public C2 C2
        {
            get => this.GetTrackedData<C2>("c2");
            set => this.SetData("c2", value);
        }
    }

    public static class TrackedObjectExtensions
    {
        public static T GetTrackedData<T>(
            this IExtendableObject extendableObject,
            string name) where T : class
        {
            var obj = extendableObject.GetData<T>(name).AsTrackable();
            obj.CastToIChangeTrackable()
                .PropertyChanged += (sender, args) =>
            {
                var trackable = (IChangeTrackable<T>)sender;
                trackable.AcceptChanges();

                extendableObject.SetData(name, trackable.GetOriginal());
            };
            return obj;
        }
    }
}
