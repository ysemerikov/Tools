using System;

namespace Tools.Core.Actions.BinaryFilesActions.FileDuplicationDetectorNamespace;

public class ByteArray
{
    public static readonly ByteArray Default = new ByteArray(Array.Empty<byte>());

    private readonly byte[] bytes;
    private int? hashCode;

    public ByteArray(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return hashCode ??= CalculateHashCode(bytes);


    }

    public override bool Equals(object? obj)
    {
        if (!(obj is ByteArray another))
            return false;

        return GetHashCode() == another.GetHashCode() // it's cached, so cheap
               && Equals(bytes, another.bytes);
    }

    public static int CalculateHashCode(byte[] bytes, int step = 1)
    {
        if (step <= 0)
            throw new Exception($"{nameof(step)} should be greater than 0, but was {step}");

        unchecked
        {
            var hash = 17;
            for (var i = 0; i < bytes.Length; i+=step)
            {
                var element = bytes[i];
                hash = hash * 31 + element;
            }

            return hash;
        }
    }

    public static bool Equals(byte[] x, byte[] y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x.Length != y.Length)
            return false;

        for (var i = 0; i < x.Length; i++)
            if (x[i] != y[i])
                return false;

        return true;
    }
}