using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Core.Logging;

namespace Tools.Core.HttpTesterNamespace
{
    public class HttpTesterStarter : StarterBase
    {
        public HttpTesterStarter(ILogger logger, ArgumentReader argumentReader) : base(logger, argumentReader)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public Task Headers()
        {
            var httpTester = GetHttpTester();
            return httpTester.GetHeaders();
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Time()
        {
            var httpTester = GetHttpTester();
            var timespans = await httpTester.TestResponseTime(10);

            var average = new TimeSpan(timespans.Sum(x => x.Ticks) / timespans.Count);
            logger.WriteLine($"Average: {average:g}");
        }

        private HttpTester GetHttpTester()
        {
            var url = argumentReader.ReadNextStringOrDefault();
            var headers = GetHeaders();

            return new HttpTester(url, headers, logger);
        }

        private IEnumerable<(string Name,string Value)> GetHeaders()
        {
            var name = default(string);
            string arg;
            while ((arg = argumentReader.ReadNextStringOrDefault()) != default)
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