using System.Collections.Generic;

namespace Tools
{
    internal class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public static readonly ByteArrayComparer Instanse = new ByteArrayComparer();
        public static readonly byte[] DefaultElement = new byte[0];

        public bool Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
                return false;

            for (var i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public int GetHashCode(byte[] bytes)
        {
            if (bytes == null || bytes == DefaultElement)
                return 0;

            unchecked
            {
                var hash = 17;
                for (var i = 0; i < bytes.Length; i++)
                {
                    var element = bytes[i];
                    hash = hash*31 + element;
                }
                return hash;
            }
        }
    }
}