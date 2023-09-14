using System.IO;
using System.Threading.Tasks;

namespace Tools.Actions.TextFileActions;

public class TextSearch : IAction
{
    private readonly string what;
    private readonly string @where;

    private static string[] NonTextFileExtensions = new string[]
    {
        ".pack", ".png", ".jpg", ".jpeg", ".svg", ".ico",
    };

    public async Task Do()
    {
        var files = Directory.EnumerateFiles(where, "*", SearchOption.AllDirectories);
        foreach (var fileName in files)
        {
            if (NonTextFileExtensions.Any(x => fileName.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                continue;

            var text = await File.ReadAllTextAsync(fileName);
            if (text.Contains(what))
            {
                Console.WriteLine(fileName.Substring(where.Length));
            }
        }
    }

    public TextSearch(string what, string? where = null)
    {
        this.what = what;
        this.where = where ?? @"D:\thm\landing";
    }
}
