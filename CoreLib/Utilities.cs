namespace CoreLib;

public static class Utilities
{
    private static readonly string[] Sizes = {"B", "KB", "MB", "GB", "TB", "PB", "EB"};

    public static string GetPrintedSize(long bytes)
    {
        var d = (double) bytes;
        foreach (var name in Sizes)
        {
            if (d < 99 || ReferenceEquals(name, Sizes[^1]))
            {
                return $"{d:0.#}{name}";
            }

            d /= 1024;
        }

        return "impossible";
    }

    public static Task OpenInNotepad(string filePath)
    {
        var process = Process.Start(@"C:\Program Files\Notepad++\notepad++.exe", $"\"{filePath}\"");
        return process.WaitForExitAsync();
    }

    public static void CreateDirectory(string path)
    {
        var directoryName = Path.GetDirectoryName(path);
        if (directoryName == null || Directory.Exists(directoryName))
        {
            return;
        }

        CreateDirectory(directoryName);
        Directory.CreateDirectory(directoryName);
    }
}
