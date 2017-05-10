using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Tools.SerializerComparer.Serializers
{
    public class SimpleDataContractSerializer<TRoot> : ISerializer<TRoot>
    {
        private static readonly Type[] DescendantTypes = typeof (TRoot).GetDescendantTypes().ToArray();

        private readonly DataContractSerializer backgroundTaskSerializer =
            new DataContractSerializer(typeof (TRoot), DescendantTypes, int.MaxValue, false, true, null);

        public string Name => "SimpleDataContractSerializer";

        public void Serialize(TRoot instance, Stream target)
        {
            backgroundTaskSerializer.WriteObject(target, instance);
        }

        public TRoot Deserialize(Stream source)
        {
            return (TRoot) backgroundTaskSerializer.ReadObject(source);
        }
    }
}