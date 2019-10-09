using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Tools.Core.FileDuplicationDetectorNamespace;
using Tools.Core.HttpTesterNamespace;
using Tools.Core.Logging;
using Tools.Core.ServiceTitan;

namespace Tools.Core
{
    public static class Program
    {
        private static readonly Type[] Starters =
        {
            typeof(FileDuplicationDetectorStarter),
            typeof(HttpTesterStarter),
            typeof(EndOfLinesFixerStarter),
            typeof(DialpadCallbackSender),
            typeof(TwilioExtender),
            typeof(TenantIdFixer),
            typeof(FuckingReportStarter),
        };

        private static async Task Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1024;

            var logger = new SimplyConsoleLogger(LoggerLevel.Debug);
            var argumentReader = new ArgumentReader(args);

            var starterType = GetStarterType(argumentReader);
            var starterMethod = GetStarterMethod(argumentReader, starterType);
            argumentReader = argumentReader.GetNextReader();

            Console.WriteLine($"Running {starterType.Name}.{starterMethod.Name}");

            var obj = starterType
                .GetConstructor(new[] {typeof(ILogger), typeof(ArgumentReader)})
                .Invoke(new object[] {logger, argumentReader});

            var stopwatch = default(Stopwatch);
            try
            {
                if (IsAsync(starterMethod))
                {
                    stopwatch = Stopwatch.StartNew();
                    await (Task) starterMethod.Invoke(obj, Array.Empty<object>());
                }
                else
                {
                    stopwatch = Stopwatch.StartNew();
                    starterMethod.Invoke(obj, Array.Empty<object>());
                }

                stopwatch.Stop();
            }
            finally
            {
                if (stopwatch != default)
                    Console.WriteLine("Finished: " + stopwatch.Elapsed);
            }
        }

        private static Type GetStarterType(ArgumentReader argumentReader)
        {
            var className = argumentReader.ReadNextStringOrDefault();
            return className == default
                ? Starters.Last()
                : Starters.Single(x => x.Name.Equals(className, StringComparison.CurrentCultureIgnoreCase));
        }

        private static MethodInfo GetStarterMethod(ArgumentReader argumentReader, Type type)
        {
            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.DeclaringType == type)
                .OrderBy(x => x.Name)
                .ToList();

            if (methods.Count == 1)
                return methods.Single();

            var methodName = argumentReader.ReadNextStringOrDefault();
            return methodName == null
                ? methods.First()
                : methods.Single(x => x.Name.Equals(methodName, StringComparison.CurrentCultureIgnoreCase));
        }

        private static bool IsAsync(MethodInfo methodInfo)
        {
            var returnType = methodInfo.ReturnType;
            return returnType == typeof(Task)
                   || returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);
        }
    }
}