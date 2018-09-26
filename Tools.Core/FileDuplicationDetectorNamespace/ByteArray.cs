using System;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Tools.Core.FileDuplicationDetectorNamespace
{
    public class ByteArray
    {
        public static readonly ByteArray Default = new ByteArray(null);

        private readonly byte[] bytes;
        private int? hashCode;

        public ByteArray(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public override int GetHashCode()
        {
            if (hashCode.HasValue)
                return hashCode.Value;

            if (bytes == null)
            {
                hashCode = 0;
                return hashCode.Value;
            }

            unchecked
            {
                var hash = 17;
                for (var i = 0; i < bytes.Length; i++)
                {
                    var element = bytes[i];
                    hash = hash*31 + element;
                }

                return (hashCode = hash).Value;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ByteArray another))
                return false;

            return Equals(bytes, another.bytes);
        }

        public static bool Equals(byte[] x, byte[] y)
        {
            if(x == null)
                return y == null;
            if (y == null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (var i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }
    }
}