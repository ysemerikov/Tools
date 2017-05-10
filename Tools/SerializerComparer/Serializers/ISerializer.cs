using System.IO;

namespace Tools.SerializerComparer.Serializers
{
    public interface ISerializer<T>
    {
        string Name { get; }

        void Serialize(T obj, Stream stream);
        T Deserialize(Stream stream);
    }
}