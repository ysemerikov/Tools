using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tools.SerializerComparer.Serializers
{
    public class DeflateBinnarySerializer<T> : ISerializer<T>
    {
        private static readonly BinaryFormatter serializer = new BinaryFormatter();

        public string Name => "DeflateBinaryFormatter";

        public void Serialize(T obj, Stream stream)
        {
            using (var deflateStream = new DeflateStream(stream, CompressionMode.Compress, true))
                serializer.Serialize(deflateStream, obj);
        }

        public T Deserialize(Stream stream)
        {
            using (var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
                return (T) serializer.Deserialize(deflateStream);
        }
    }
}