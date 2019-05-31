using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tools.Core.Logging;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace Tools.Core.ServiceTitan
{
    public class TwilioExtender : StarterBase
    {
        public const string OutputFilename = @"C:\Users\ysemerikov\Downloads\semerikov1-result.csv";
        private static readonly Regex messageRegex = new Regex("^\"Debugging event for tenant (\\d*). Call Sid:'(\\w+)'. Error code: 32011. Message: '503 service error with sip:\\+(\\d+)@cust216.auth.bandwidth.com:5012'.\"$", RegexOptions.Compiled);
        private static TwilioRestClient Client = new TwilioRestClient(Secrets.Twilio.ProductionUsername, Secrets.Twilio.ProductionPassword);
        
        public async Task Do()
        {
            var csvLines = File.ReadAllLines(argumentReader.ReadNextStringOrDefault() ?? @"C:\Users\ysemerikov\Downloads\semerikov1.csv");
            var parsed = csvLines
                .Skip(1)
                .Select(l => l.Split(','))
                .Select(a => a.Length == 2 ? a[1] : null)
                .Select(ParseLog)
                .Where(x => x.CallSid != null)
                .ToList();

            Console.WriteLine($"Total: {parsed.Count}, With Id: {parsed.Count(x => x.TenantId.HasValue)}");

            var queue = new ConcurrentQueue<Line>();
            await parsed.ToAsyncEnumerable().ParallelForEachAsync(async x =>
            {
                var call = await CallResource.FetchAsync(x.CallSid, client: Client);
                var line = await GetLine(call, x.TenantId);
                queue.Enqueue(line);
            }, 64);

            var lines = queue
                .OrderBy(x => x.TenantId)
                .ThenBy(x => x.IsInbound)
                .ThenBy(x => x.From)
                .ThenBy(x => x.CallSid)
                .Select(x => x.ToString());

            File.WriteAllLines(
                argumentReader.ReadNextStringOrDefault() ?? OutputFilename,
                new[] {Line.CsvHeader}.Concat(lines));
        }

        private async Task<Line> GetLine(CallResource call, long? tenantId)
        {
            var isFirstLeg = string.IsNullOrEmpty(call.ParentCallSid);
            var parentCall = isFirstLeg ? null : await CallResource.FetchAsync(call.ParentCallSid, client: Client);
            bool isInbound;
            var from = call.From;
            var to = call.To;

            if (parentCall != null)
            {
                if (parentCall.Direction == "inbound")
                {
                    isInbound = true;
                    to = parentCall.To;
                }
                else
                {
                    isInbound = false;
                }
            }
            else
            {
                if (call.Direction == "outbound-api")
                {
                    isInbound = false;
                    to = "unknown";
                }
                else
                {
                    throw new Exception($"Unknown call direction: {call.Sid}.");
                }
            }

            return new Line
            {
                CallSid = call.Sid,
                TenantId = tenantId,
                From = from,
                To = to,
                IsFirstLeg = isFirstLeg,
                IsInbound = isInbound,
                Date = call.DateCreated ?? call.StartTime.Value,
            };
        }

        private (long? TenantId, string CallSid) ParseLog(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "(");

            var match = messageRegex.Match(message);
            if (!match.Success)
                return default;

            var tenantIdString = match.Groups[1].Value;
            var callSid = match.Groups[2].Value;

            if (!callSid.StartsWith("CA"))
                throw new ArgumentException(message, nameof(message));

            return string.IsNullOrEmpty(tenantIdString) ? (default(long?), callSid) : (long.Parse(tenantIdString), callSid);
        }

        public TwilioExtender(ILogger logger, ArgumentReader argumentReader) : base(logger, argumentReader)
        {
        }
    }

    public class Line
    {
        private static readonly Regex sipRegex = new Regex("^sip:(\\+1\\d{10,10})@.+$", RegexOptions.Compiled);
        public string CallSid { get; set; }
        public long? TenantId { get; set; }
        public bool IsInbound { get; set; }
        public bool IsFirstLeg { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }

        public override string ToString()
        {
            var to = To;
            var match = sipRegex.Match(to);
            if (match.Success)
                to = match.Groups[1].Value;
            return $"{CallSid},{TenantId},{From},{to},{(IsInbound ? "inbound" : "outbound")},{(IsFirstLeg ? "first leg" : "second leg")},{Date.AddHours(-12):s}";
        }

        public static string CsvHeader = "CallSid,TenantId,From,To,Direction,Leg,Time PST";
    }
}