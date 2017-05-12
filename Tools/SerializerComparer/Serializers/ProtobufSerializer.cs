using System.IO;
using ProtoBuf;
using Tools.SerializerComparer.Models;

namespace Tools.SerializerComparer.Serializers
{
    internal class ProtobufSerializer<T> : ISerializer<T>
    {
        public string Name => "ProtobufSerializer";

        public void Serialize(T obj, Stream stream)
        {
            Serializer.Serialize(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            return Serializer.Deserialize<T>(stream);
        }
    }
}