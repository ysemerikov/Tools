using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
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

        public async Task GetTcpHeaders()
        {
            if (url.Scheme != "http")
                throw new NotSupportedException();

            var bytes = new List<byte>();

            using (var tcpClient = new TcpClient(url.Host, url.Port))
            using (var stream = tcpClient.GetStream())
            {
                var builder = new StringBuilder();
                builder.AppendLine($"GET {url.PathAndQuery} HTTP/1.1");
                builder.AppendLine($"Host: {url.Host}");
                builder.AppendLine("Connection: close");
                foreach (var header in headers)
                {
                    builder.AppendLine($"{header.Name}: {header.Value}");
                }
                builder.AppendLine();

                var data = Encoding.ASCII.GetBytes(builder.ToString());
                await stream.WriteAsync(data, 0, data.Length);

                var state = 0;
                var nextByte = -1;

                while ((nextByte = stream.ReadByte()) >= 0)
                {
                    switch (state)
                    {
                        case 0:
                        case 2:
                            if (nextByte == '\r')
                                state++;
                            else
                                state = 0;
                            break;
                        case 1:
                        case 3:
                            if (nextByte == '\n')
                                state++;
                            else
                                state = 0;
                            break;
                        default:
                            throw new Exception();
                    }

                    bytes.Add((byte) nextByte);

                    if (state == 4)
                        break;
                }
            }

            logger.WriteLine(Encoding.ASCII.GetString(bytes.ToArray(), 0, bytes.Count));
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