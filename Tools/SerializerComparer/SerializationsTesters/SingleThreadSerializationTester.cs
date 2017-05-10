using System.Collections.Generic;
using Tools.SerializerComparer.Models;
using Tools.SerializerComparer.Serializers;

namespace Tools.SerializerComparer.SerializationsTesters
{
    internal class SingleThreadSerializationTester<T> : SerializationTesterBase<T> where T : IHasEquals<T>
    {
        public SingleThreadSerializationTester(ISerializer<T> serializer)
            : base(serializer)
        {
        }

        protected override void Serialize(ISerializer<T> serializer, List<SerializationData<T>> data)
        {
            foreach (var obj in data)
            {
                serializer.Serialize(obj.Model, obj.Stream);
            }
        }

        protected override void Deserialize(ISerializer<T> serializer, List<SerializationData<T>> data)
        {
            foreach (var obj in data)
            {
                obj.DeserializedModel = serializer.Deserialize(obj.Stream);
            }
        }
    }
}