using System.Threading.Tasks;
using Tools.Core.Logging;

namespace Tools.Core.Actions.BinaryFilesActions.FileDuplicationDetectorNamespace;

public class FileDuplicationDetectorStarter : IAction
{
    public Task Do()
    {
        var inputDirectories = new[]
        {
            @"C:\Users\ysemerikov\FROM ASUS\",
            @"C:\Users\ysemerikov\YandexDisk",
        };
        var outputDirectory = @"C:\Users\ysemerikov\" + nameof(FileDuplicationDetector);

        var logger = new SimplyConsoleLogger(LoggerLevel.Debug);

        var detector = new FileDuplicationDetector(inputDirectories, outputDirectory, logger);
        return detector.Detect();
    }
}
