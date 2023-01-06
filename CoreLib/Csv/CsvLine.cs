namespace CoreLib.Csv;

public class CsvLine : IReadOnlyList<string>
{
    private readonly string[] data;
    private readonly CsvFile file;

    internal CsvLine(CsvFile file, string[] data)
    {
        this.file = file;
        this.data = data;
    }

    public string this[string name]
    {
        get
        {
            for (var i = 0; i < file.ColumnNames.Length; i++)
            {
                if (name.Equals(file.ColumnNames[i], StringComparison.Ordinal))
                {
                    return data[i];
                }
            }

            throw new ArgumentException($"Column '{name}' doesn't exist.");
        }
    }

    public string this[int index] => data[index];

    public IEnumerator<string> GetEnumerator()
    {
        return data.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => data.Length;
}
