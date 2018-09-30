using System.Threading.Tasks;
using Tools.Core.Logging;

namespace Tools.Core.FileDuplicationDetectorNamespace
{
    public class FileDuplicationDetectorStarter : StarterBase
    {
        public FileDuplicationDetectorStarter(ILogger logger, ArgumentReader argumentReader) : base(logger, argumentReader)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public Task Detect()
        {
            var inputDirectory = argumentReader.ReadNextStringOrDefault()?.Split(',')
                                 ?? new[]
                                 {
                                     @"C:\Users\ysemerikov\FROM ASUS\",
                                     @"C:\Users\ysemerikov\YandexDisk",
                                 };
            var outputDirectory = argumentReader.ReadNextStringOrDefault()
                                  ?? @"C:\Users\ysemerikov\" + nameof(FileDuplicationDetector);

            var detector = new FileDuplicationDetector(inputDirectory, outputDirectory, logger);
            return detector.Detect();
        }
    }
}