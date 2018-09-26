using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tools.Core.FileDuplicationDetectorNamespace;
using Tools.Core.Logging;

namespace Tools.Core
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            var argumentReader = new ArgumentReader(args);
            Func<ArgumentReader, Task> action;
            switch (argumentReader.GetString(0))
            {
                case null:
                case nameof(FileDuplicationDetector):
                    action = FileDuplicationDetector;
                    break;
                default:
                    throw new ArgumentException($"{args[0]} is unknown command.", nameof(args));
            }

            var startNew = Stopwatch.StartNew();
            await action(argumentReader);
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
    }
}