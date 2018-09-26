using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tools.Core.FileDuplicationDetectorNamespace;
using Tools.Core.HttpTesterNamespace;
using Tools.Core.Logging;

namespace Tools.Core
{
    public static class Program
    {
        private static readonly ILogger Logger = new SimplyConsoleLogger(LoggerLevel.Debug);

        private static async Task Main(string[] args)
        {
            var argumentReader = new ArgumentReader(args);
            Func<ArgumentReader, Task> action;
            switch (argumentReader.GetString(0))
            {
                case null:
                case nameof(HttpTester_Headers):
                    action = HttpTester_Headers;
                    break;
                case nameof(HttpTester_Time):
                    action = HttpTester_Time;
                    break;
                case nameof(FileDuplicationDetector):
                    action = FileDuplicationDetector;
                    break;
                default:
                    throw new ArgumentException($"{args[0]} is unknown command.", nameof(args));
            }

            var startNew = Stopwatch.StartNew();
            await action(argumentReader);
            Console.WriteLine();
            Console.WriteLine("Finished: " + startNew.Elapsed);
        }

        private static Task FileDuplicationDetector(ArgumentReader argumentReader)
        {
            var inputDirectory = argumentReader.GetString(1)?.Split(',') ?? new[]
            {
                @"C:\Users\ysemerikov\FROM ASUS\",
                @"C:\Users\ysemerikov\YandexDisk",
            };
            var outputDirectory = argumentReader.GetString(2) ?? @"C:\Users\ysemerikov\" + nameof(FileDuplicationDetector);

            var detector = new FileDuplicationDetector(inputDirectory, outputDirectory, new SimplyConsoleLogger(LoggerLevel.Debug));
            return detector.Detect();
        }

        private static Task HttpTester_Headers(ArgumentReader argumentReader)
        {
            var httpTester = GetHttpTester(argumentReader);
            return httpTester.GetHeaders();
        }

        private static async Task HttpTester_Time(ArgumentReader argumentReader)
        {
            var httpTester = GetHttpTester(argumentReader);
            var timespans = await httpTester.TestResponseTime(10);

            var average = new TimeSpan(timespans.Sum(x => x.Ticks) / timespans.Count);
            Logger.WriteLine($"Average: {average:g}");
        }

        private static HttpTester GetHttpTester(ArgumentReader argumentReader)
        {
            var url = argumentReader.GetString(1);
            var headers = GetHeaders(argumentReader.Skip(2));

            var httpTester = new HttpTester(url, headers, Logger);
            return httpTester;
        }

        private static IEnumerable<(string Name,string Value)> GetHeaders(IEnumerable<string> arguments)
        {
            var name = default(string);
            foreach (var arg in arguments)
            {
                if (name == default)
                {
                    name = arg;
                }
                else
                {
                    yield return (name, arg);
                    name = default;
                }
            }

            if (name != default)
                throw new ArgumentException("There should be even number of arguments.");
        }
    }
}