using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Tools.SerializerComparer.Serializers
{
    public class Deflate2DataContractSerializer<TRoot> : ISerializer<TRoot>
    {
        private static readonly Type[] DescendantTypes = typeof (TRoot).GetDescendantTypes().ToArray();

        private readonly DataContractSerializer backgroundTaskSerializer =
            new DataContractSerializer(typeof (TRoot), DescendantTypes, int.MaxValue, false, true, null);

        public string Name => "Deflate2DataContractSerializer";

        public void Serialize(TRoot instance, Stream target)
        {
            using (var deflateStream = new DeflateStream(target, CompressionMode.Compress, true))
            using (var binaryWriter = XmlDictionaryWriter.CreateBinaryWriter(deflateStream, null, null, false))
                backgroundTaskSerializer.WriteObject(binaryWriter, instance);
        }

        public TRoot Deserialize(Stream source)
        {
            using (var deflateStream = new DeflateStream(source, CompressionMode.Decompress, true))
            using (
                var binaryReader = XmlDictionaryReader.CreateBinaryReader(deflateStream, XmlDictionaryReaderQuotas.Max))
                return (TRoot) backgroundTaskSerializer.ReadObject(binaryReader);
        }
    }
}