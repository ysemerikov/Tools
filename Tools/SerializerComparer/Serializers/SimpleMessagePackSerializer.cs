using System.IO;
using MsgPack.Serialization;

namespace Tools.SerializerComparer.Serializers
{
    public class MsgSerializer<T> : ISerializer<T>
    {
        private static readonly MessagePackSerializer<T> serializer = MessagePackSerializer.Get<T>();
        public string Name => "MessagePackSerializer";

        public void Serialize(T obj, Stream stream)
        {
            serializer.Pack(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            return serializer.Unpack(stream);
        }
    }
}