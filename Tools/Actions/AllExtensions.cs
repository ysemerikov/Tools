using System.IO;
using System.Threading.Tasks;
using CoreLib;

namespace Tools.Actions;

public class AllExtensions : IAction
{
    private readonly string folder;

    public Task Do()
    {
        var grouped = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            .AsParallel()
            .Select(x =>
            {
                var extension = Path.GetExtension(x);
                if (string.IsNullOrEmpty(extension))
                    extension = "NO EXTENSION";

                var length = new FileInfo(x).Length;

                return (extension, length);
            })
            .GroupBy(x => x.extension, x => x.length)
            .Select(x => (x.Key, Count: x.Count(), Sum: x.Sum()))
            .OrderByDescending(x => x.Sum);

        foreach (var g in grouped)
        {
            Console.WriteLine($"{g.Key}\t{g.Count}\t{Utilities.GetPrintedSize(g.Sum)}");
        }

        return Task.CompletedTask;
    }

    public AllExtensions(string? folder = null)
    {
        this.folder = folder ?? @"D:\thm\landing";
    }
}
