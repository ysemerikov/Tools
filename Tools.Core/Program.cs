using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Tools.Core.Actions.NetworkActions;
using Tools.Core.FileDuplicationDetectorNamespace;
using Tools.Core.Logging;
using Tools.Core.ServiceTitan;
using Tools.Core.TextFileActions;

namespace Tools.Core
{
    public static class Program
    {
        private static readonly Type[] Starters =
        {
            typeof(FileDuplicationDetectorStarter),
            typeof(EndOfLinesFixer),
            typeof(FuckingReportStarter),
            typeof(HttpRequestsLogger),
        };

        private static async Task Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1024;

            var action = new TcpRequestSender();

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await action.Do();
            }
            finally
            {
                Console.WriteLine("Finished: " + stopwatch.Elapsed);
            }
        }
    }
}