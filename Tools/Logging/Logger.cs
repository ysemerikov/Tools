using System;
using System.Text;

namespace Tools.Logging;

public class SimplyConsoleLogger : ILogger
{
    private readonly LoggerLevel minWriteLevel;

    public void WriteLine(string? line, LoggerLevel? loggerLevel = LoggerLevel.Info)
    {
        if (loggerLevel >= minWriteLevel)
            Console.WriteLine(line);
    }

    public SimplyConsoleLogger(LoggerLevel minWriteLevel)
    {
        Console.OutputEncoding = Encoding.UTF8;

        this.minWriteLevel = minWriteLevel;
    }
}
