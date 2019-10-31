using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tools.Core.Actions.NetworkActions
{
    public class HttpRequestsLogger : IAction
    {
        public async Task Do()
        {
            var server = new HttpListener
            {
                Prefixes = {"http://*:80/", "https://*:443/"}
            };
            server.Start();

            for (var i = 0;; i++)
            {
                var connection = await ReadOneConnection(server);
                
                var filename = $"{connection.Start:HH-mm-ss.ff}_{i}";
                var color = connection.Exception == null ? ConsoleColor.Green : ConsoleColor.Red;

                using (var writer = File.Open($@"C:\servicetitan\{filename}.txt", FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    await connection.CopyToAsync(writer);

                Log($"{filename}\t{connection.RemoteEndPoint}", color);
            }
        }

        private static async Task<Connection> ReadOneConnection(HttpListener listener)
        {
            var ctx = await listener.GetContextAsync();
            var connection = new Connection
            {
                Start = DateTime.Now,
                RemoteEndPoint = ctx.Request.RemoteEndPoint,
                HttpMethod = ctx.Request.HttpMethod,
                RawUrl = ctx.Request.RawUrl,
            };

            try
            {
                var headers = new List<(string, string)>();
                foreach (var key in ctx.Request.Headers.AllKeys)
                {
                    var values = ctx.Request.Headers.GetValues(key);
                    if (values != null)
                        headers.AddRange(values.Select(value => (key, value)));
                }

                var content = new MemoryStream();
                await ctx.Request.InputStream.CopyToAsync(content);
                content.Position = 0;

                connection.Headers = headers;
                connection.Content = content.ToArray();
            }
            catch (Exception e)
            {
                connection.Exception = e;
            }
            finally
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.Close();
            }

            return connection;
        }

        private static void Log(string message, ConsoleColor? color = null)
        {
            var was = Console.ForegroundColor;

            if (color.HasValue)
                Console.ForegroundColor = color.Value;

            Console.WriteLine(message);

            if (color.HasValue)
                Console.ForegroundColor = was;
        }

        private class Connection
        {
            public DateTime Start { get; set; }
            public EndPoint RemoteEndPoint { get; set; }
            public List<(string Key, string Value)> Headers { get; set; }
            public byte[] Content { get; set; }
            public Exception Exception { get; set; }
            public string HttpMethod { get; set; }
            public string RawUrl { get; set; }

            public async Task CopyToAsync(Stream output)
            {
                using (var writer = new StreamWriter(output, Encoding.UTF8, 2048, true))
                {
                    await writer.WriteLineAsync($"{HttpMethod} {RawUrl}");
                    if (Headers != null)
                        foreach (var header in Headers)
                            await writer.WriteLineAsync($"{header.Key}: {header.Value}");

                    await writer.WriteLineAsync();

                    if (Exception != null)
                        await writer.WriteAsync(Exception.ToString());
                }

                if (Exception == null && Content != null)
                    await output.WriteAsync(Content, 0, Content.Length);
            }
        }
    }
}