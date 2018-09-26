using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Tools.Core.Logging;

namespace Tools.Core.HttpTesterNamespace
{
    public class HttpTester
    {
        private readonly Uri url;
        private readonly List<(string Name, string Value)> headers;
        private readonly ILogger logger;

        public HttpTester(string url, IEnumerable<(string Name, string Value)> headers, ILogger logger)
        {
            this.logger = logger;
            this.url = new Uri(url);
            this.headers = headers.ToList();
        }

        public async Task<List<TimeSpan>> TestResponseTime(int attempts = 1, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            var httpClient = new HttpClient();
            foreach (var header in headers)
                httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);

            var result = new List<TimeSpan>(attempts);

            for (var i = 0; i < attempts; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                using (await httpClient.GetAsync(url, httpCompletionOption))
                {
                    var elapsed = stopwatch.Elapsed;

                    logger.WriteLine(elapsed.ToString("g"));
                    result.Add(elapsed);
                }
            }

            return result;
        }

        public async Task GetHeaders()
        {
            var httpClient = new HttpClient();
            foreach (var header in headers)
                httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);

            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                var strings = response.Headers.SelectMany(HeaderToStrings);
                foreach (var header in strings)
                {
                    logger.WriteLine(header);
                }
            }
        }

        private static IEnumerable<string> HeaderToStrings(KeyValuePair<string, IEnumerable<string>> header)
        {
            var name = header.Key;
            var values = header.Value.ToList();

            if (values.Count == 0)
                yield return name;

            foreach (var value in values)
                yield return $"{name}: {value}";
        }
    }
}