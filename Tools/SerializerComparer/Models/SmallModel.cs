using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tools.SerializerComparer.Models
{
    [Serializable]
    [DataContract]
    public class SmallModel : IHasEquals<SmallModel>
    {
        [DataMember]
        public int Int { get; set; }
        [DataMember]
        public long Long { get; set; }
        [DataMember]
        public string String { get; set; }
        [DataMember]
        public DateTime DateTime { get; set; }
        [DataMember]
        public byte[] Bytes { get; set; }

        public static string RandomString(Random rand, int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var array = new char[length];
            for (var i = 0; i < length; i++)
                array[i] = chars[rand.Next(chars.Length)];

            return new string(array);
        }

        public static IEnumerable<SmallModel> Generate(int count)
        {
            var rand = new Random();
            var maxTicks = System.DateTime.MaxValue.Ticks;

            for (var i = 0; i < count; i++)
            {
                var bytes = new byte[128];
                rand.NextBytes(bytes);

                var kind = (DateTimeKind) (i%3);
                var ticks = Math.Abs(GetLong(rand));
                while (ticks > maxTicks)
                    ticks /= 10;

                yield return new SmallModel
                {
                    Int = rand.Next(),
                    Long = GetLong(rand),
                    DateTime = new DateTime(ticks, kind),
                    String = RandomString(rand, 32),
                    Bytes = bytes
                };
            }
        }

        private static long GetLong(Random rand)
        {
            // ReSharper disable once RedundantCast
            return (((long) rand.Next()) << 32) | ((long) rand.Next());
        }

        public bool IsEquals(SmallModel b)
        {
            if (Int != b.Int || Long != b.Long || String != b.String || DateTime.Ticks != b.DateTime.Ticks || DateTime.Kind != b.DateTime.Kind ||
                Bytes.Length != b.Bytes.Length)
                return false;

            for (var i = 0; i < Bytes.Length; i++)
            {
                if (Bytes[i] != b.Bytes[i])
                    return false;
            }

            return true;
        }
    }
}