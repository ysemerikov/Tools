using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace Tools.Core.ServiceTitan.TwilioDebugger
{
    public class TwilioDebuggerReport : IAction
    {
        public async Task Do()
        {
            var client = new TwilioRestClient(Secrets.Twilio.ProductionUsername, Secrets.Twilio.ProductionPassword);

            var reportStorage = ReportStorage.Restore(@"C:\servicetitan\tw-report-storage.bin");
            var data = (await GetAllNotifications(client))
                .Where(x => new DateTime(2019, 12, 16, 20, 0, 0) < x.DateCreated &&
                            x.DateCreated < new DateTime(2019, 12, 16, 22, 0, 0));

            var counter = 0;
            foreach (var @event in data.Where(x => !reportStorage.Has(x)))
            {
                if (@event.CallSid.StartsWith("MM")
                    || @event.CallSid.StartsWith("SM")
                )
                    continue;

                var report = await TwilioLogic.GetReport(@event, client);
                if (report == null || report.ErrorType == ErrorType.Unknown)
                {
                    Console.WriteLine();
                    Console.WriteLine(JsonConvert.SerializeObject(@event));
                    break;
                }

                await reportStorage.Add(report);
                Console.Write($"\r        \r{++counter}");
            }

            AnalyzeResult(reportStorage);
        }

        private static void AnalyzeResult(ReportStorage reportStorage)
        {
            var allReports = reportStorage.GetAllReports()
                .Where(x => x.ErrorType == ErrorType.CallWasntConnected)
                .DistinctBy(x => x.CallSid)
                .Select(TenantLogic.GetTenant)
                .Where(x => x != null)
                //.Where(x => !x.Tenant.StartsWith("id-") && x.Tenant != "tulsa" && x.Tenant != "okc")
                .GroupBy(x => x)
                .Select(x => $"{x.Key},{x.Count()}")
                .OrderBy(x => x)
                .ToList();

            File.AppendAllLines(@"C:\servicetitan\report.csv", allReports);
        }

        private static async Task<List<NotificationResource>> GetAllNotifications(TwilioRestClient client)
        {
            var filename = @"C:\servicetitan\twilioNotifications.json";

            if (File.Exists(filename))
                try
                {
                    var json = await File.ReadAllTextAsync(filename);
                    return JsonConvert.DeserializeObject<List<NotificationResource>>(json);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            var all = (await NotificationResource.ReadAsync(new ReadNotificationOptions {PageSize = 1000}, client))
                .ToList();
            await File.WriteAllTextAsync(filename, JsonConvert.SerializeObject(all));
            return all;
        }
    }
}