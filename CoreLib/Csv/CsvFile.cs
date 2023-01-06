namespace CoreLib.Csv;

public class CsvFile : IReadOnlyList<CsvLine>
{
    public readonly string[] ColumnNames;
    private readonly CsvLine[] data;

    public CsvFile(string[] columnNames, IEnumerable<string[]> data)
    {
        ColumnNames = columnNames;
        this.data = data.Select(x => new CsvLine(this, x)).ToArray();
    }

    public IEnumerator<CsvLine> GetEnumerator()
    {
        return data.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => data.Length;
    public CsvLine this[int index] => data[index];
}
