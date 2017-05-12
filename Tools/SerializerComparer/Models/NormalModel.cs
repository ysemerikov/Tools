using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tools.SerializerComparer.Models
{
    [Serializable]
    [DataContract]
    public class NormalModel : IHasEquals<NormalModel>
    {
        [DataMember]
        public string Str { get; set; }

        [DataMember]
        public List<string> Strs { get; set; }

        [DataMember]
        public List<int> Ints { get; set; }

        [DataMember]
        public byte[][] Bytes{ get; set; }

        public bool IsEquals(NormalModel b)
        {
            if (Str != b.Str || Strs.Count != b.Strs.Count || Ints.Count != b.Ints.Count ||
                Bytes.Length != b.Bytes.Length)
                return false;

            for (int i = 0; i < Strs.Count; i++)
            {
                if (Strs[i] != b.Strs[i])
                    return false;
            }

            for (int i = 0; i < Ints.Count; i++)
            {
                if (Ints[i] != b.Ints[i])
                    return false;
            }

            for (int i = 0; i < Bytes.Length; i++)
            {
                if (Bytes[i].Length != b.Bytes[i].Length)
                    return false;

                for (int j = 0; j < Bytes[i].Length; ++j)
                {
                    if(Bytes[i][j] != b.Bytes[i][j])
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<NormalModel> Generate(int count)
        {
            var rand = new Random();
            for (int i = 0; i < count; i++)
            {
                yield return GenerateOne(rand);
            }
        }

        private static NormalModel GenerateOne(Random rand)
        {
            const int size = 64*1024;

            const int strSize = size/32;
            const int strsSize = size/8;
            const int intsSize = size/4;
            const int bytesSize = size - strSize - strsSize - intsSize;

            var strsCount = rand.Next(64, 128);
            var intsCount = intsSize/4;

            var bytes = new byte[16][];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = GenerateByteArray(rand, bytesSize/bytes.Length);
            }

            return new NormalModel
            {
                Str = SmallModel.RandomString(rand, strSize),
                Strs =
                    Enumerable.Repeat(0, strsCount)
                        .Select(_ => SmallModel.RandomString(rand, strsSize/strsCount))
                        .ToList(),
                Ints = Enumerable.Repeat(0, intsCount).Select(_ => rand.Next()).ToList(),
                Bytes = bytes
            };
        }

        private static byte[] GenerateByteArray(Random rand, int count)
        {
            var bytes = new byte[count];
            rand.NextBytes(bytes);
            return bytes;
        }
    }
}
