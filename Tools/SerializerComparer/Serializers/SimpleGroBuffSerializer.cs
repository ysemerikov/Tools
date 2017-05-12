using System.IO;
using GroBuf;
using GroBuf.DataMembersExtracters;

namespace Tools.SerializerComparer.Serializers
{
    internal class SimpleGroBuffSerializer<T> : ISerializer<T>
    {
        private static readonly Serializer serializer = new Serializer(new PropertiesExtractor(),
            options: GroBufOptions.PackReferences);
        public string Name => "SimpleGroBuffSerializer";

        public void Serialize(T obj, Stream stream)
        {
            var bytes = serializer.Serialize(obj);
            stream.Write(bytes, 0, bytes.Length);
        }

        public T Deserialize(Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return serializer.Deserialize<T>(bytes);
        }
    }
}