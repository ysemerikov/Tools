namespace Tools.Logging;

public interface ILogger
{
    void WriteLine(string? line, LoggerLevel? loggerLevel = LoggerLevel.Info);
}
