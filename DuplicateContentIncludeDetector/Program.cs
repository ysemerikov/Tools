using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DuplicateContentIncludeDetector
{
    internal static class Program
    {
        private static readonly Regex Regex = new Regex("Include=\"(.+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static void Main(string[] args)
        {
            var directoryPath = args.FirstOrDefault() ?? ".";

            try {
                Print(FindDuplicates(directoryPath));
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        private static IEnumerable<Elem> FindDuplicates(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists)
                throw new Exception(string.Format("Directory '{0}' not exists", directory.FullName));

            return directory.EnumerateFiles("*.csproj", SearchOption.AllDirectories)
                .Select(info => new Elem(info, FindDuplicates(File.ReadAllLines(info.FullName)).ToList()))
                .Where(o => o.BadLines.Count > 0);
        }

        private static void Print(IEnumerable<Elem> duplicates)
        {
            var finded = false;

            foreach (var duplicate in duplicates) {
                Console.WriteLine("{0}:", duplicate.Project.Name);
                foreach (var badLine in duplicate.BadLines) {
                    Console.WriteLine("{0}\t{1}", badLine.Number, badLine.Line);
                }
                Console.WriteLine();
                finded = true;
            }

            if (finded == false)
                Console.WriteLine("Duplicates not detected");
        }

        private static IEnumerable<BadLine> FindDuplicates(string[] lines)
        {
            var set = new HashSet<string>();
            for (var i = 0; i < lines.Length; i++) {
                var match = Regex.Match(lines[i]);
                if (match.Success) {
                    var fileName = match.Groups[1].Value;
                    if (set.Contains(fileName)) {
                        yield return new BadLine(lines[i], i + 1);
                    }
                    else {
                        set.Add(fileName);
                    }
                }
            }
        }

        private class Elem
        {
            public readonly List<BadLine> BadLines;
            public readonly FileInfo Project;

            public Elem(FileInfo project, List<BadLine> badLines)
            {
                Project = project;
                BadLines = badLines;
            }
        }

        private class BadLine
        {
            public readonly string Line;
            public readonly int Number;

            public BadLine(string line, int number)
            {
                Line = line;
                Number = number;
            }
        }
    }
}