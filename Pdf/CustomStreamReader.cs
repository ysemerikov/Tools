namespace Pdf;

public interface ICustomStreamReader
{
    int Position { get; }

    string ReadLine();
    byte[] ReadBytes(int length);

    bool TryRead(IEnumerable<char> chars);
    bool TryReadLine(string? chars = default);
    bool TryReadPositiveInt(out int value);
    bool IsFinished();

    IDisposable Forget();
    int Forget(Action<ICustomStreamReader> func);
    void ForgetIf(Func<ICustomStreamReader, bool> func);
    T Forget<T>(Func<ICustomStreamReader, T> func);
}

public class CustomStreamReader : ICustomStreamReader
{
    private readonly byte[] data;

    public int Position { get; private set; }

    public CustomStreamReader(byte[] data)
        : this(data, 0)
    {}

    private CustomStreamReader(byte[] data, int position)
    {
        this.data = data;
        Position = position;
    }

    public string ReadLine()
    {
        var index = Array.IndexOf(data, (byte)'\n', Position);
        if (index < 0)
            index = data.Length - 1;

        if (index == Position)
        {
            Position += 1;
            return string.Empty;
        }

        var stringBy = data[index - 1] == '\r'
            ? index - 1
            : index;
        var chars = data.SubCharArray(Position, stringBy - Position);

        Position = index + 1;

        return new string(chars);
    }

    public byte[] ReadBytes(int length)
    {
        CheckLength(length);

        var result = data.SubArray(Position, length);
        Position += length;
        return result;
    }

    public bool TryRead(IEnumerable<char> expected)
    {
        var count = 0;
        foreach (var c in expected)
            if (data[Position + (count++)] != c)
                return false;

        Position += count;
        return true;
    }

    public bool TryReadLine(string? expected = default)
    {
        expected ??= string.Empty;
        return TryRead(expected.Concat("\r\n")) || TryRead(expected.Concat("\n"));
    }

    public bool TryReadPositiveInt(out int value)
    {
        var wasAny = false;
        value = 0;

        while ('0' <= data[Position] && data[Position] <= '9')
        {
            value = value * 10 + data[Position++] - '0';
            wasAny = true;
        }

        return wasAny;
    }

    public bool IsFinished()
    {
        return Position == data.Length;
    }

    #region Forgot

    public IDisposable Forget()
    {
        var position = Position;
        return new DisposeAction(() => Position = position);
    }

    public int Forget(Action<ICustomStreamReader> func)
    {
        var pos = Position;
        return Forget(r =>
        {
            func(this);
            return r.Position - pos;
        });
    }

    public void ForgetIf(Func<ICustomStreamReader, bool> func)
    {
        var pos = Position;
        if (func(this))
            Position = pos;
    }

    public T Forget<T>(Func<ICustomStreamReader, T> func)
    {
        using (Forget())
            return func(this);
    }

    #endregion

    private void CheckLength(in int length)
    {
        if (Position + length > data.Length)
            throw new ArgumentException($"Can't read so many: {Position} + {length}");
    }

    private class DisposeAction : IDisposable
    {
        private readonly Action action;

        public DisposeAction(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            action();
        }
    }
}
