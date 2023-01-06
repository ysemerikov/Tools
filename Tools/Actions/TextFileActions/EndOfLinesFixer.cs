﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tools.Actions.TextFileActions;

public class EndOfLinesFixer : IAction
{
    public Task Do()
    {
        var extensions = new[] { "cs", "config", "csproj", "js", "cshtml", "resx" };
        return Do(extensions, true, @"C:\servicetitan\app");
    }

    private static async Task Do(string[] extensions, bool recursive, params string[] directoryPaths)
    {
        var directories = directoryPaths.Select(path =>
        {
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
                throw new Exception($"Directory '{directory.FullName}' doesn't exists.");
            return directory;
        });

        var count = await Fix(extensions, recursive, directories);
        Console.WriteLine("Total {0} files affected.", count);
    }

    private static string ToPattern(string ext)
    {
        if (ext.StartsWith("*."))
            return ext;
        if (ext.StartsWith("."))
            return "*" + ext;
        return "*." + ext;
    }

    private static async Task<int> Fix(string[] extensions, bool recursive, IEnumerable<DirectoryInfo> directories)
    {
        var tasks =
            extensions
                .AsParallel()
                .SelectMany(e =>
                    directories.SelectMany(d => d.EnumerateFiles(ToPattern(e), recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)))
                .Select(Fix);

        var results = await Task.WhenAll(tasks);
        return results.Count(t => t);
    }

    private static async Task<bool> Fix(FileInfo fileInfo)
    {
        using (var stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
            var oldFile = await ReadAllBytesAsync(stream);
            var newFile = oldFile.Where(b => b != '\r').ToList();

            if (newFile.Count == oldFile.Length)
                return false;

            stream.SetLength(newFile.Count);
            stream.Seek(0, SeekOrigin.Begin);

            await stream.WriteAsync(newFile.ToArray(), 0, newFile.Count);
        }

        return true;
    }

    private static async Task<byte[]> ReadAllBytesAsync(FileStream stream)
    {
        var fileLength = stream.Length;
        if (fileLength > Int32.MaxValue)
            throw new IOException();

        var count = (int) fileLength;
        var bytes = new byte[count];
        var index = 0;
        while (count > 0) {
            var n = await stream.ReadAsync(bytes, index, count);
            if (n == 0)
                throw new IOException();
            index += n;
            count -= n;
        }
        return bytes;
    }
}
