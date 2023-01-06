using CsvHelper;
using CsvHelper.Configuration;

namespace CoreLib.Csv;

public static class StaticCsvReader
{
    public static async Task<CsvFile> ReadAsync(
        string csvFilePath,
        string delimiter = ",")
    {
        using var stream = File.OpenText(csvFilePath);
        return await ReadAsync(stream, delimiter);
    }

    public static async Task<CsvFile> ReadAsync(
        StreamReader stream,
        string delimiter = ",")
    {
        var parser = new CsvParser(stream, new CsvConfiguration(CultureInfo.InvariantCulture) {Delimiter = delimiter, IgnoreBlankLines = true});

        if (!await parser.ReadAsync())
        {
            throw new ArgumentException("Stream is empty.");
        }

        var columnNames = parser.Record!;
        var list = new List<string[]>();
        while (await parser.ReadAsync())
        {
            list.Add(parser.Record!);
        }

        return new CsvFile(columnNames, list);
    }
}
