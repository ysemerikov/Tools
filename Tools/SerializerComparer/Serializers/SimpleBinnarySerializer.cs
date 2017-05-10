using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tools.SerializerComparer.Serializers
{
    public class SimpleBinnarySerializer<T> : ISerializer<T>
    {
        private static readonly BinaryFormatter serializer = new BinaryFormatter();

        public string Name => "SimpleBinaryFormatter";

        public void Serialize(T obj, Stream stream)
        {
            serializer.Serialize(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            return (T) serializer.Deserialize(stream);
        }
    }
}