using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ForgottenFilesDetector
{
    internal static class Program
    {
        private static readonly Regex regex =
            new Regex("(?:None|Compile|Content|EmbeddedResource|EntityDeploy) Include=\"(.+?)\"", RegexOptions.Compiled);

        private static readonly string[] excluded =
        {
            ".csproj.DotSettings"
        };

        private static void Main(string[] args)
        {
            var root = args.FirstOrDefault() ?? @".";

            var projects = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories);
            Console.WriteLine("Найдено проектов: {0}", projects.Length);

            foreach (var project in projects)
            {
                var csproj = new FileInfo(project);
                var forgottenFiles = GetForgottenFiles(csproj, true).FilterExcluded();

                if (forgottenFiles.Any())
                {
                    var lastIndex = csproj.Name.LastIndexOf(".");
                    var projectName = csproj.Name.Substring(0, lastIndex);
                    Console.WriteLine("{0}{1}:", Environment.NewLine, projectName);

                    foreach (var source in forgottenFiles)
                    {
                        Console.WriteLine(source);
                    }
                }
            }
        }

        private static IEnumerable<string> GetForgottenFiles(FileInfo csproj, bool ignoreCase)
        {
            var comparer = ignoreCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture;
            return GetAllFiles(csproj).Except(GetAllIncludedFiles(csproj), comparer).Where(f => f != csproj.FullName);
        }

        private static IEnumerable<string> GetAllFiles(FileInfo fileInfo)
        {
            return fileInfo.Directory.GetFiles("*", SearchOption.AllDirectories).Select(f => f.FullName);
        }

        private static IEnumerable<string> GetAllIncludedFiles(FileInfo csproj)
        {
            var matches = regex.Matches(File.ReadAllText(csproj.FullName));
            return matches.Cast<Match>().Select(m => Path.Combine(csproj.Directory.FullName, m.Groups[1].Value));
        }

        private static IEnumerable<string> FilterExcluded(this IEnumerable<string> enumerable)
        {
            return enumerable.Where(fileName => !excluded.Any(fileName.EndsWith));
        }
    }
}