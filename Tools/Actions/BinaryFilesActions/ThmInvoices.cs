using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tools.Actions.BinaryFilesActions;

public class ThmInvoices : IAction
{
    private static readonly Regex FileNameRegex = new (@"^(\d\d\d\d-\d\d-\d\d)-(\d\d\d\d-\d\d-\d\d)\.pdf$");
    private const string ResultFolderName = "_result";

    private readonly string root;

    public Task Do()
    {
        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Select(x =>
                {
                    var folder = Path.GetDirectoryName(x);

                    if (Path.GetDirectoryName(folder) != root)
                        throw new Exception($"File {x.Substring(root.Length + 1)} isn't expected.");

                    return (FolderName: Path.GetFileName(folder), FilePath: x);
                })
                .Where(x => x.FolderName != ResultFolderName)
                .Select(x =>
                {

                    var fileName = Path.GetFileName(x.FilePath);
                    var match = FileNameRegex.Match(fileName);
                    if (!match.Success)
                        throw new Exception($"File {x.FilePath.Substring(root.Length + 1)} has unexpected name.");

                    var date = DateOnly.Parse(match.Groups[2].Value).AddDays(-1);
                    var expectedMonth = $"{date.Year}-{date.Month:00}";
                    if (!fileName.StartsWith(expectedMonth, StringComparison.Ordinal))
                        throw new Exception($"File {x.FilePath.Substring(root.Length + 1)} has weird name (month).");

                    return (x.FilePath, NewName: $"{expectedMonth} {x.FolderName}.pdf");
                })
                .ToArray()
            ;

        Console.WriteLine(files.Length);
        if (files.Length == 0)
            return Task.CompletedTask;

        var resultFolderPath = Path.Combine(root, ResultFolderName);
        if (!Directory.Exists(resultFolderPath))
            Directory.CreateDirectory(resultFolderPath);

        foreach (var (path, newName) in files)
        {
            File.Copy(path, Path.Combine(resultFolderPath, newName), false);
        }

        return Task.CompletedTask;
    }

    public ThmInvoices(string folderPath)
    {
        root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folderPath));
    }
}
