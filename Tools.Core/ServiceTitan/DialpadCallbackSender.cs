using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Tools.Core.Logging;
using HttpClient = System.Net.Http.HttpClient;

namespace Tools.Core.ServiceTitan
{
    public class DialpadCallbackSender : StarterBase
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task Do()
        {
            var url = new Uri(Secrets.Dialpad.ProductionCallbackUrl + "140914307"); // 4866 for telecom
            for (var message = ReadMessage();
                !string.IsNullOrEmpty(message) && !message.Equals("exit", StringComparison.InvariantCultureIgnoreCase);
                message = ReadMessage())
            {
                var result = await Send(url, message);
                Console.WriteLine(result);
                Console.WriteLine();
            }
        }

        private static string ReadMessage()
        {
            Console.WriteLine(">>>");
            var sb = new StringBuilder();
            for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.Append(line);
            }

            return sb.ToString();
        }

        private static async Task<string> Send(Uri url, string message)
        {
            message = Sign(message);

            var content = new StringContent(message, Encoding.UTF8, "application/jwt");
            var stopwatch = Stopwatch.StartNew();
            using (var response = await client.PostAsync(url, content))
            {
                Console.WriteLine(stopwatch.Elapsed);

                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.StatusCode + ": " + result);
                return result;
            }
        }

        private static string Sign(string message)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secrets.Dialpad.ProductionCallbackKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(credentials);

            var payload = JwtPayload.Deserialize(message);
            var secToken = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(secToken);
        }

        public DialpadCallbackSender(ILogger logger, ArgumentReader argumentReader) : base(logger, argumentReader)
        {
        }
    }
}