using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;

namespace Tools.SerializerComparer.Serializers
{
    public class DeflateDataContractSerializer<TRoot> : ISerializer<TRoot>
    {
        private static readonly Type[] DescendantTypes = typeof (TRoot).GetDescendantTypes().ToArray();

        private readonly DataContractSerializer backgroundTaskSerializer =
            new DataContractSerializer(typeof (TRoot), DescendantTypes, int.MaxValue, false, true, null);

        public string Name => "DeflateDataContractSerializer";

        public void Serialize(TRoot instance, Stream target)
        {
            using (var deflateStream = new DeflateStream(target, CompressionMode.Compress, true))
                backgroundTaskSerializer.WriteObject(deflateStream, instance);
        }

        public TRoot Deserialize(Stream source)
        {
            using (var deflateStream = new DeflateStream(source, CompressionMode.Decompress, true))
                return (TRoot) backgroundTaskSerializer.ReadObject(deflateStream);
        }
    }
}