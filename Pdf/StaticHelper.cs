using System;

namespace Pdf
{
    public static class StaticHelper
    {
        public static byte[] SubArray(this byte[] source, int sourceIndex, int length)
        {
            if (sourceIndex < 0)
                throw new ArgumentException("must be >= 0.", nameof(sourceIndex));
            if (length < 0)
                throw new ArgumentException("must be >= 0.", nameof(length));
            if (sourceIndex + length > source.Length)
                throw new ArgumentException("array is too short.", nameof(length));

            if (length == 0)
                return Array.Empty<byte>();

            var destination = new byte[length];
            Array.Copy(source, sourceIndex, destination, 0, length);
            return destination;
        }

        public static char[] SubCharArray(this byte[] source, int sourceIndex, int length)
        {
            if (sourceIndex < 0)
                throw new ArgumentException("must be >= 0.", nameof(sourceIndex));
            if (length < 0)
                throw new ArgumentException("must be >= 0.", nameof(length));
            if (sourceIndex + length > source.Length)
                throw new ArgumentException("array is too short.", nameof(length));

            if (length == 0)
                return Array.Empty<char>();

            var destination = new char[length];
            Array.Copy(source, sourceIndex, destination, 0, length);
            return destination;
        }
    }
}