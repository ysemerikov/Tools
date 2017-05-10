using System.Collections.Generic;
using System.Linq;
using Tools.SerializerComparer.Models;
using Tools.SerializerComparer.Serializers;

namespace Tools.SerializerComparer.SerializationsTesters
{
    internal class MultiThreadSerializationTester<T> : SerializationTesterBase<T> where T : IHasEquals<T>
    {
        public MultiThreadSerializationTester(ISerializer<T> serializer)
            : base(serializer)
        {
        }

        protected override void Serialize(ISerializer<T> serializer, List<SerializationData<T>> data)
        {
            data.AsParallel().ForAll(obj => serializer.Serialize(obj.Model, obj.Stream));
        }

        protected override void Deserialize(ISerializer<T> serializer, List<SerializationData<T>> data)
        {
            data.AsParallel().ForAll(obj => obj.DeserializedModel = serializer.Deserialize(obj.Stream));
        }
    }
}